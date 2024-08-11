using System.Net;
using System.Net.Sockets;

namespace RxSpy.Grpc.Tests;

public static class TcpPort
{
    public static int GetFreePort()
    {
        TcpListener listener = new(IPAddress.Loopback, 0);
    
        try
        {
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            return port;
        }
        finally
        {
            listener.Stop();
        }
    }

    public static bool IsPortInUse(int port)
    {
        try
        {
            TcpListener listener = new(System.Net.IPAddress.Loopback, port);
            listener.Start();
            listener.Stop();
            return false;
        }
        catch (SocketException)
        {
            return true;
        }
    }
}