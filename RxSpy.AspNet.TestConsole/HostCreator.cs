using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Mime;
using System.Net.Sockets;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace RxSpy.AspNet.TestConsole;

public static class HostCreator
{
    private static int TcpPort { get; set; } = -1;

    public static IWebHost CreateHost(string[] args)
    {
        TcpPort = GetFreeTcpPort();
        var host = WebHost.CreateDefaultBuilder(args);

        host.ConfigureAppConfiguration((context, config) =>
        {
            config
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);
        });

        host.ConfigureKestrel((context, options) =>
        {
            options.ListenLocalhost(TcpPort, opts =>
            {
                opts.Protocols = HttpProtocols.Http2 | HttpProtocols.Http3;
            });

            options.ConfigureHttpsDefaults(httpsOptions =>
            {
                var certSettings = new CertificateSettings();
                context.Configuration.GetSection("CertificateSettings").Bind(certSettings);

                var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                certStore.Open(OpenFlags.ReadOnly);
                var certs = certStore.Certificates.Find(
                    X509FindType.FindByThumbprint,
                    certSettings.ServerCertificateThumbprint,
                    validOnly: false);

                if (certs.Count > 0)
                {
                    httpsOptions.ServerCertificate = certs[0];
                }

                certStore.Close();
            });
        });

        host.Configure(app =>
        {
            app.UseHttpsRedirection();
            app.UseCors(CorsPolicyName.TOKEN_LOCAL_HOST_HTTP);
            app.UseRouting();
            app.UseRxSpy();
        });

        host.ConfigureServices((context, services) =>
        {
            services.Configure<CertificateSettings>(context.Configuration.GetSection("CertificateSettings"));

            services
                .AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
                .AddCertificate(opts =>
                {
                    opts.AllowedCertificateTypes = CertificateTypes.All;
                    // opts.AllowedCertificateTypes = CertificateTypes.Chained;
                    // opts.RevocationMode = X509RevocationMode.NoCheck;
                    opts.Events = new CertificateAuthenticationEvents
                    {
                        OnCertificateValidated = async context =>
                        {
                            Claim[] claims = [];
                            var identity = new ClaimsIdentity(claims, context.Scheme.Name);
                            context.Principal = new ClaimsPrincipal(identity);
                            context.Success();
                            await Task.CompletedTask;
                        },
                        OnAuthenticationFailed = async context =>
                        {
                            // context.ClientCertificate.Tumbprint
                            // context.ClientCertificate.Subject
                            context.NoResult();
                            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            context.Response.ContentType = MediaTypeNames.Text.Plain;
                            await context.Response.WriteAsync(context.Exception.ToString());
                        }
                    };
                });

            services.AddCertificateForwarding(options =>
            {
                options.CertificateHeader = "X-ARR-ClientCert";
                options.HeaderConverter = (headerValue) =>
                {
                    var bytes = Convert.FromBase64String(headerValue);
                    return new X509Certificate2(bytes);
                };
            });

            services.AddHttpsRedirection(opt =>
            {
                opt.HttpsPort = TcpPort;
                opt.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
            });

            services.AddRxSpy();
            services.AddRouting();

            services.AddCors(corsOptions =>
            {
                corsOptions.AddPolicy(name: CorsPolicyName.TOKEN_LOCAL_HOST_HTTP,
                    policy =>
                    {
                        policy
                            .WithOrigins("localhost")
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });

            
        });

        return host.Build();
    }

    private static int GetFreeTcpPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}