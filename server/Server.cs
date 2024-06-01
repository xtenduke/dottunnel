namespace server;
using System.Net;
using System.Net.Sockets;
using tunnel;

public class Server(string listenAddress, int tunnelPort, int clientPort)
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
    
    private static void HandleClient(TcpClient client, TcpClient proxyConnection)
    {
        Console.WriteLine("HandleClient");
        var clientStream = client.GetStream();
        var proxyStream = proxyConnection.GetStream();
        Task.Run(() => ConnectionHelper.ForwardData(clientStream, proxyStream));
        Task.Run(() => ConnectionHelper.ForwardData(proxyStream, clientStream));
    }

    private static TcpListener CreateListener(string listenAddress, int listenPort)
    {
        var localAddress = IPAddress.Parse(listenAddress);
        var server = new TcpListener(localAddress, listenPort);
        server.Start();
        Console.WriteLine("Server listening on {0}", listenPort);
        return server;
    }
}