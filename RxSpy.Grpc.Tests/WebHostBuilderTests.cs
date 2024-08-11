using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace RxSpy.Grpc.Tests;

public sealed class CustomMiddlewareTests : IDisposable
{
    private readonly HttpClient _client;

    public CustomMiddlewareTests(ITestOutputHelper output)
    {
        var cert = SelfSignedCertificateFactory.Create();
        var webHostBuilder = new WebHostBuilder();

        webHostBuilder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddXUnit(output);
            logging.SetMinimumLevel(LogLevel.Debug);
        });

        webHostBuilder.ConfigureKestrel(serverOptions =>
        {
            // Configure Kestrel to use the self-signed certificate
            serverOptions.ListenLocalhost(5001, opts =>
            {
                opts.UseHttps(cert);
                opts.Protocols = HttpProtocols.Http2 | HttpProtocols.Http3;
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
            app.UseHealthChecks("/hc");

            app.Run(async context =>
            {
                var weatherForecast = new[] { "Sunny", "Cloudy", "Rainy" };

                var response = new
                {
                    Data = weatherForecast,
                    Debug = env.IsDevelopment() ? new
                    {
                        Environment = env.EnvironmentName,
                        Path = context.Request.Path,
                        QueryString = context.Request.QueryString.ToString()
                    } : null
                };

                var jsonResponse = JsonSerializer.Serialize(response);

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(jsonResponse);
            });
        });

        webHostBuilder.ConfigureServices(services =>
        {
            services.AddHealthChecks();
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                options.HttpsPort = 5001;
            });
        });

        webHostBuilder.UseEnvironment("Development");

        var server = new TestServer(webHostBuilder);
        _client = server.CreateClient();
    }

    [Fact]
    public async Task Get_RedirectsToHttps()
    {
        // Act: Make a GET request to the HTTP endpoint
        var response = await _client.GetAsync("http://localhost/");

        // Assert: Verify the response is a redirect
        Assert.Equal(StatusCodes.Status307TemporaryRedirect, (int)response.StatusCode);
        Assert.Equal("https://localhost:5001/", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task Get_ReturnsWeatherForecast()
    {
        // Act: Make a GET request to the root endpoint
        var response = await _client.GetAsync("https://localhost:5001/");

        // Assert: Verify the response
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();

        Assert.Contains("Sunny", responseString);
        Assert.Contains("Cloudy", responseString);
        Assert.Contains("Rainy", responseString);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}