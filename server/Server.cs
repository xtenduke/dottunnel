namespace server;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using tunnel;

public class Server(String listenAddress, Int32 tunnelPort, Int32 clientPort)
{
    private Tunnel? _tunnel;
    
    public async Task Run()
    {
        // listen for connections on tunnelPort and clientPort
        // when bytes are received, write to the other 
        
        // Start listening for client requests.
        // naming conventions suck
        var clientListener = CreateListener(listenAddress, clientPort);
        while (true)
        {
            // accept a new connection every loop
            var client = await clientListener.AcceptTcpClientAsync();
            Console.WriteLine("Client connected on {0}", clientPort);
            await HandleClient(client);
        }

    }
    
    private async Task HandleClient(TcpClient client)
    {
        // assuming connected
        // Just make me backoff instead of trying to connect when first client connects
        if (_tunnel == null)
        {
            Console.WriteLine("Created new tunnel connection.....");
            var listener = CreateListener(listenAddress, tunnelPort);
            var tunnelConnection = await listener.AcceptTcpClientAsync();
            _tunnel = new Tunnel(tunnelConnection);
        }

        var connectionId = _tunnel.AddConnection(client);
        
        try
        {
            await _tunnel.WriteIntoTunnel(client, connectionId);
        }
        catch (IOException)
        {
            Console.WriteLine("Caught IOException writing to tunnel");
        }
                        
        try
        {
            await _tunnel.WriteFromTunnel();
        }
        catch (IOException)
        {
            Console.WriteLine("Caught IOException writing to tunnel");
        }
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