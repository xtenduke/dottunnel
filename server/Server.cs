namespace server;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using tunnel;

public class Server(String listenAddress, Int32 tunnelPort, Int32 clientPort)
{
    private TcpClient? tunnelConnection;
    private Tunnel? tunnel;
    
    public void Run()
    {
        // listen for connections on tunnelPort and clientPort
        // when bytes are received, write to the other 
        
        // Start listening for client requests.
        // naming conventions suck
        TcpListener clientListener = CreateListener(listenAddress, clientPort);
        while (true)
        {
            // accept a new connection every loop
            TcpClient client = clientListener.AcceptTcpClient();
            Console.WriteLine("Server connected on {0}", clientPort);
            HandleClient(client);
        }
    }
    
    private void HandleClient(TcpClient client)
    {
        // assuming connected
        NetworkStream clientStream = client.GetStream();

        if (tunnel == null || tunnelConnection == null)
        {
            tunnelConnection = CreateListener(listenAddress, tunnelPort).AcceptTcpClient();
            tunnel = new Tunnel(tunnelConnection.GetStream());
        }

        string connectionId = tunnel.AddConnection(clientStream);
        
        // assuming connected
        NetworkStream proxyStream = tunnelConnection.GetStream();

        
        tunnel.WriteIntoTunnel(clientStream, connectionId);
        Console.WriteLine("Server wrote client to agent");
        tunnel.WriteFromTunnel();
        Console.WriteLine("Server wrote proxy to user");
    }

    private static TcpListener CreateListener(String listenAddress, Int32 listenPort)
    {
        IPAddress localAddress = IPAddress.Parse(listenAddress);
        TcpListener server = new TcpListener(localAddress, listenPort);
        server.Start();
        Console.WriteLine("Server listening on {0}", listenPort);
        return server;
    }
    
    public static byte[] TrimEnd(byte[] array)
    {
        int lastIndex = Array.FindLastIndex(array, b => b != 0);
        Array.Resize(ref array, lastIndex + 1);
        return array;
    }
}