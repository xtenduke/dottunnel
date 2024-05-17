using System.Net.Mail;

namespace server;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Server(String listenAddress, Int32 tunnelPort, Int32 clientPort)
{
    private TcpClient? proxyConnection;
    
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

        if (proxyConnection == null || !proxyConnection.Connected)
        {
            proxyConnection = CreateListener(listenAddress, tunnelPort).AcceptTcpClient();    
        }
        
        // assuming connected
        NetworkStream proxyStream = proxyConnection.GetStream();

        
        CrossStreams(clientStream, proxyStream);
        Console.WriteLine("Server wrote client to agent");
        CrossStreams(proxyStream, clientStream);
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
    
    private static void CrossStreams(NetworkStream source, NetworkStream destination)
    {
        Console.WriteLine("Writing stream");
        Byte[] buffer = new byte[1024];
        int i = -1;
        while (i != 0)
        {
            i = source.Read(buffer, 0, buffer.Length);
            buffer = TrimEnd(buffer);
            destination.Write(buffer, 0, buffer.Length);
            // if the last message was smaller than max buffer we can assume that its done
            if (buffer.Length != 1024)
            {
                Console.WriteLine("WillBreak");
                break;
            }
        }
    }
    
    public static byte[] TrimEnd(byte[] array)
    {
        int lastIndex = Array.FindLastIndex(array, b => b != 0);
    
        Array.Resize(ref array, lastIndex + 1);
    
        return array;
    }
}