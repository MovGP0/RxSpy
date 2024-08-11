using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Grpc.Core;
using Grpc.Net.Client;
using RxSpy.Protobuf.Events;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RxSpy.Factories;
using Xunit.Abstractions;

namespace RxSpy.Grpc.Tests;

public sealed class RxSpyGrpcServiceTests : IDisposable
{
    private readonly int _port;
    private readonly IHost _server;
    private readonly GrpcChannel _channel;
    private readonly RxSpyService.RxSpyServiceClient _client;
    private readonly X509Certificate2 _cert;
    private readonly ILoggerFactory _loggerFactory;

    
    
    public RxSpyGrpcServiceTests(ITestOutputHelper output)
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
            }),
            LoggerFactory = _loggerFactory
        });

        _client = new RxSpyService.RxSpyServiceClient(_channel);
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

    private void SetupWebHostBuilder(IWebHostBuilder webHostBuilder, ITestOutputHelper output)
    {
        webHostBuilder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddXUnit(output);
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddFilter("Grpc", LogLevel.Debug);
        });

        webHostBuilder.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(_port, opts =>
            {
                opts.UseHttps(_cert);
                opts.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
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
                options.HttpsPort = _port;
            });
        });

        webHostBuilder.UseEnvironment("Development");
    }

    [Fact]
    public async Task GetEvents_StreamsEventsSuccessfully()
    {
        var log = _loggerFactory.CreateLogger<RxSpyGrpcServiceTests>();
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(15));

        // Precondition check: test if host returns healthy
        using var healthClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = ValidateCertificate
        });
        var response = await healthClient.GetAsync($"https://localhost:{_port}/hc", cts.Token);
        var responseString = await response.Content.ReadAsStringAsync(cts.Token);

        response.ShouldSatisfyAllConditions(
            r => r.IsSuccessStatusCode.ShouldBeTrue(),
            _ => responseString.ShouldBe("Healthy"));

        // Arrange: subscribe to the server events
        var getEvents = _client.GetEvents(new Empty(), cancellationToken: cts.Token);
        getEvents.ShouldNotBeNull();

        // Act
        _ = Task.Run(() => SimulateServerEvents(cts.Token), cts.Token);

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
        await cts.CancelAsync();
    }

    private async Task SimulateServerEvents(CancellationToken cancellationToken)
    {
        var method = GetType().GetMethod(
            nameof(SimulateServerEvents), 
            BindingFlags.NonPublic | BindingFlags.Instance, 
            null,
            new[]
            {
                typeof(CancellationToken)
            },
            null);

        Debug.Assert(method != null);

        var handler = new RxSpyGrpcEventHandler();

        var operatorCreatedEvent = new RxSpy.Events.OperatorCreatedEvent
        {
            EventId = 1,
            EventTime = 124567L,
            Id = 1,
            Name = "Operator1",
            CallSite = CallSiteFactory.Create(new StackFrame()),
            OperatorMethod = MethodInfoFactory.Create(method)
        };
        handler.OnCreated(operatorCreatedEvent);
        await Task.Delay(100, cancellationToken);

        var onNextEvent = new RxSpy.Events.OnNextEvent
        {
            EventId = 2,
            EventTime = 124567L,
            OperatorId = 1,
            Value = "SampleValue",
            ValueType = "string",
            Thread = 124
        };
        handler.OnNext(onNextEvent);
        await Task.Delay(100, cancellationToken);

        var onCompletedEvent = new RxSpy.Events.OnCompletedEvent
        {
            EventId = 3,
            EventTime = 124567L,
            OperatorId = 1
        };
        handler.OnCompleted(onCompletedEvent);
        await Task.Delay(100, cancellationToken);
    }

    public void Dispose()
    {
        var stopTask = _server.StopAsync();
        stopTask.Wait(TimeSpan.FromSeconds(5));

        _server.Dispose();
        _channel.Dispose();
        _cert.Dispose();
        _loggerFactory.Dispose();
    }
}