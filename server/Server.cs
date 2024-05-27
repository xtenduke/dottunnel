namespace server;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using tunnel;

public class Server(String listenAddress, Int32 tunnelPort, Int32 clientPort)
{
    public void Run()
    {
        var clientListener = CreateListener(listenAddress, clientPort);
        var proxyListener = CreateListener(listenAddress, tunnelPort);

        while (true)
        {
            // accept client connections
            // when client connection is available, accept a tunnel connection
            var clientConnection = clientListener.AcceptTcpClient();
            var proxyConnection = proxyListener.AcceptTcpClient();
            Console.WriteLine("Client connected on {0}", clientPort);
            Console.WriteLine("Proxy connected on {0}", tunnelPort);
                
            ThreadPool.QueueUserWorkItem(delegate { HandleClient(clientConnection, proxyConnection); });
        }
    }
    
    private void HandleClient(TcpClient client, TcpClient proxyConnection)
    {
        Console.WriteLine("HandleClient");
        var clientStream = client.GetStream();
        var proxyStream = proxyConnection.GetStream();
        Task.Run(() => ConnectionManager.ForwardData(clientStream, proxyStream));
        Task.Run(() => ConnectionManager.ForwardData(proxyStream, clientStream));
    }

    private static TcpListener CreateListener(String listenAddress, Int32 listenPort)
    {
        var localAddress = IPAddress.Parse(listenAddress);
        var server = new TcpListener(localAddress, listenPort);
        server.Start();
        Console.WriteLine("Server listening on {0}", listenPort);
        return server;
    }
}