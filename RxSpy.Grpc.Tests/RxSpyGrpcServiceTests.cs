using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Grpc.Core;
using Grpc.Net.Client;
using RxSpy.Protobuf.Events;
using Google.Protobuf.WellKnownTypes;
using Grpc.Health.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace RxSpy.Grpc.Tests;

public sealed class RxSpyGrpcServiceTests : IDisposable
{
    private readonly TestServer _server;
    private readonly GrpcChannel _channel;
    private readonly RxSpyService.RxSpyServiceClient _client;
    private readonly X509Certificate2 _cert;
    private readonly ILoggerFactory _loggerFactory;

    public RxSpyGrpcServiceTests(ITestOutputHelper output)
    {
        // Generate a self-signed certificate
        _cert = SelfSignedCertificateFactory.Create();

        // Arrange
        IWebHostBuilder webHostBuilder = new WebHostBuilder();

        webHostBuilder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddXUnit(output);
            logging.SetMinimumLevel(LogLevel.Debug);
        });

        webHostBuilder.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(225, opts =>
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
                // see https://learn.microsoft.com/en-us/aspnet/core/grpc/health-checks
                endpoints.MapGrpcHealthChecksService()
                    .AllowAnonymous();

                endpoints.MapGrpcService<RxSpyGrpcService>();
            });

            app.UseHealthChecks("/hc");
        });

        webHostBuilder.ConfigureServices(services =>
        {
            services.AddHealthChecks();
            services.AddGrpcHealthChecks();

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
                        .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
                });
            });

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
                options.HttpsPort = 225;
            });
        });

        webHostBuilder.UseEnvironment("Development");

        _server = new TestServer(webHostBuilder);
        _loggerFactory = _server.Services.GetRequiredService<ILoggerFactory>();

        bool ValidateCertificate(
            HttpRequestMessage message,
            X509Certificate2? cert,
            X509Chain? chain,
            SslPolicyErrors errors)
        {
            return cert is not null
                   && cert.Equals(_cert);
        }

        _channel = GrpcChannel.ForAddress("https://localhost:225", new GrpcChannelOptions
        {
            // Configure HttpClient to trust the certificate
            HttpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = ValidateCertificate
            })
        });

        _client = new RxSpyService.RxSpyServiceClient(_channel);
    }

    [Fact]
    public async Task GetEvents_StreamsEventsSuccessfully()
    {
        var log = _loggerFactory.CreateLogger<RxSpyGrpcServiceTests>();
        var timeout = TimeSpan.FromSeconds(10);
        using var cts = new CancellationTokenSource(timeout);

        // Precondition check: test if host returns healthy
        var healthClient = _server.CreateClient();
        var response = await healthClient.GetAsync("https://localhost:225/hc", cts.Token);
        var responseString = await response.Content.ReadAsStringAsync(cts.Token);

        response.ShouldSatisfyAllConditions(
            r => r.IsSuccessStatusCode.ShouldBeTrue(),
            _ => responseString.ShouldBe("Healthy"));

        // Precondition check: test the GRPC service
        var channel = GrpcChannel.ForAddress("https://localhost:225");
        var client = new Health.HealthClient(channel);
        var grpcResponse = await client.CheckAsync(new HealthCheckRequest());
        grpcResponse.Status.ShouldBe(HealthCheckResponse.Types.ServingStatus.Serving);

        // Arrange: subscribe to the server events
        var getEvents = _client.GetEvents(new Empty(), cancellationToken: cts.Token);
        getEvents.ShouldNotBeNull();

        // Act
        _ = Task.Run(SimulateServerEvents);

        var receivedEvents = 0;

        await foreach (var rxSpyEvent in getEvents.ResponseStream.ReadAllAsync(cts.Token))
        {
            log.LogInformation("Received event: {Event}", rxSpyEvent);

            rxSpyEvent.ShouldNotBeNull();
            receivedEvents++;
            // Here, you can also add specific assertions based on event type, etc.
            if (receivedEvents >= 3) // Assuming we expect at least 3 events for the test
            {
                break;
            }
        }

        // Assert
        receivedEvents.ShouldBe(3);
    }

    private async Task SimulateServerEvents()
    {
        await Task.Delay(100);

        var handler = new RxSpyGrpcEventHandler();

        handler.OnCreated(new RxSpy.Events.OperatorCreatedEvent
        {
            EventId = 1,
            EventTime = 124567L,
            Id = 1,
            Name = "Operator1"
        });

        await Task.Delay(100);

        handler.OnNext(new RxSpy.Events.OnNextEvent
        {
            EventId = 2,
            EventTime = 124567L,
            OperatorId = 1,
            Value = "SampleValue",
            ValueType = "string"
        });

        await Task.Delay(100);

        handler.OnCompleted(new RxSpy.Events.OnCompletedEvent
        {
            EventId = 3,
            EventTime = 124567L,
            OperatorId = 1
        });
    }

    public void Dispose()
    {
        _channel.Dispose();
        _server.Dispose();
    }
}