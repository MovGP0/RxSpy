using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RxSpy.HelloWorld;
using Xunit.Abstractions;

namespace RxSpy.Grpc.Tests;

public sealed class GrpcTests : IDisposable
{
    private readonly int _port;
    private readonly IHost _server;
    private readonly GrpcChannel _channel;
    private readonly Greeter.GreeterClient _client;
    private readonly X509Certificate2 _cert;
    private readonly ILoggerFactory _loggerFactory;

    public GrpcTests(ITestOutputHelper output)
    {
        _port = TcpPort.GetFreePort();

        // Generate a self-signed certificate
        _cert = SelfSignedCertificateFactory.Create();

        // Arrange

        // Note: GRPC requires a full server, rather than a TestServer
        _server = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webHostBuilder =>
            {
                SetupWebHostBuilder(webHostBuilder, output);
            })
            .Build();

        _server.Start();

        _loggerFactory = _server.Services.GetRequiredService<ILoggerFactory>();

        _channel = GrpcChannel.ForAddress($"https://localhost:{_port}", new GrpcChannelOptions
        {
            // Configure HttpClient to trust the certificate
            HttpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = ValidateCertificate
            })
        });

        _client = new Greeter.GreeterClient(_channel);
    }

    private void SetupWebHostBuilder(IWebHostBuilder webHostBuilder, ITestOutputHelper output)
    {
        webHostBuilder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddXUnit(output);
            logging.SetMinimumLevel(LogLevel.Debug);
        });

        webHostBuilder.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(_port, opts =>
            {
                opts.UseHttps(_cert);
                opts.Protocols = HttpProtocols.Http2;
            });
        });

        webHostBuilder.Configure(app =>
        {
            var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowGrpc");

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcHealthChecksService().AllowAnonymous();
                endpoints.MapGrpcService<GreeterService>();
            });
        });

        webHostBuilder.ConfigureServices(services =>
        {
            services.Configure<HealthCheckPublisherOptions>(options =>
            {
                options.Delay = TimeSpan.Zero;
                options.Period = TimeSpan.FromSeconds(5);
            });

            services.AddHealthChecks();
            services.AddGrpcHealthChecks()
                .AddCheck("Hello", () => HealthCheckResult.Healthy("World"));

            services.AddRouting();
            services.AddGrpc();

            services.AddCors(corsOptions =>
            {
                corsOptions.AddPolicy("AllowGrpc", builder =>
                {
                    builder
                        .WithOrigins("http://localhost", "https://localhost")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders(
                            "Grpc-Status",
                            "Grpc-Message",
                            "Grpc-Encoding",
                            "Grpc-Accept-Encoding");
                });
            });

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
                options.HttpsPort = _port;
            });
        });

        webHostBuilder.UseEnvironment("Development");
    }

    private bool ValidateCertificate(
        HttpRequestMessage message,
        X509Certificate2? cert,
        X509Chain? chain,
        SslPolicyErrors errors)
    {
        return cert is not null
               && cert.Equals(_cert);
    }

    internal sealed class GreeterService : Greeter.GreeterBase
    {
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }
    }

    [Fact(Skip = "Health check is not working; always returns NotFound (uninitialized)")]
    public async Task ShouldReturnValidHealthCheck()
    {
        // arrange
        var client = new Health.HealthClient(_channel);
        
        // wait for the health check to be ready
        await Task.Delay(TimeSpan.FromSeconds(1));

        // act
        var grpcResponse = await client.CheckAsync(new HealthCheckRequest());

        // assert
        grpcResponse.Status.ShouldBe(HealthCheckResponse.Types.ServingStatus.Serving);
    }

    [Fact]
    public async Task ShouldReturnHelloWorld()
    {
        // arrange
        var message = new HelloRequest
        {
            Name = "GreeterClient"
        };

        // act
        var reply = await _client.SayHelloAsync(message);

        // assert
        reply.Message.ShouldBe("Hello GreeterClient");
    }

    public void Dispose()
    {
        var stopServer = _server.StopAsync();
        stopServer.Wait(TimeSpan.FromSeconds(5));

        _server.Dispose();
        _channel.Dispose();
        _cert.Dispose();
        _loggerFactory.Dispose();
    }
}