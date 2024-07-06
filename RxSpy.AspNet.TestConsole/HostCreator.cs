using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace RxSpy.AspNet.TestConsole;

public static class HostCreator
{
    private static int TcpPort { get; set; } = -1;

    public static IWebHost CreateHost(string[] args)
    {
        var host = WebHost.CreateDefaultBuilder(args);
        host.ConfigureKestrel(options =>
        {
            TcpPort = GetFreeTcpPort();
            options.ListenLocalhost(TcpPort, opts =>
            {
                opts.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
            });
        });

        host.Configure(app =>
        {
            app.UseCors(CorsPolicyName.TOKEN_LOCAL_HOST_HTTP);
            app.UseRouting();
            app.UseRxSpy();
        });

        host.ConfigureServices(services =>
        {
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