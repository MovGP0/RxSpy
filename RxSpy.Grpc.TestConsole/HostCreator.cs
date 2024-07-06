using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace RxSpy.Grpc.TestConsole;

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
            app.UseCors(CorsPolicyName.TOKEN_LOCAL_HOST_RGPC);
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<RxSpyGrpcService>();
            });
        });

        host.ConfigureServices(services =>
        {
            services.AddGrpc();
            services.AddRouting();
            services.AddCors(corsOptions =>
            {
                corsOptions.AddPolicy(name: CorsPolicyName.TOKEN_LOCAL_HOST_RGPC,
                    policy =>
                    {
                        policy
                            .WithOrigins("localhost")
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
                    });
            });

            services.AddSingleton<IRxSpyEventHandler, RxSpyGrpcEventHandler>();
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