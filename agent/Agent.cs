using System.Net;

namespace agent;

using System.Net.Sockets;
using tunnel;

// Source address is the source we are connecting to
// Destination is the server 
public class Agent(String sourceAddress, Int32 sourcePort, String proxyAddress, Int32 proxyPort)
{
    private Tunnel? _tunnel;
    
    // Open a connection to the destination
    public async Task Run()
    {
        // Connect to the proxy
        var tunnelClient = new TcpClient();
        await tunnelClient.ConnectAsync(proxyAddress, proxyPort);
        Console.WriteLine("Connected over tunnel at {0}:{1}", proxyAddress, proxyPort);
        
        // Connect to the source
        
        // Take bytes off tunnel - get the connectionId
        // Open new connection to Source and send request
        // Get response from source, using connectionId to send the wrapped messages back to the tunnel
        
        // init the tunnel
        _tunnel = new Tunnel(tunnelClient);

        // Single connection from tunnel, we are always reading off it
        var connectionMap = new Dictionary<string, Connection>();

        while (true)
        {
            var buffer = new byte[Tunnel.MaxSerializedBufferSize];

            var i = -1;
            var tunnelStream = tunnelClient.GetStream();
            tunnelStream.ReadTimeout = Tunnel.ReadTimeoutMs;
            while (i != 0)
            {
                Console.WriteLine("Agent waiting to read from tunnel");
                try
                {
                    i = tunnelStream.Read(buffer, 0, Tunnel.MaxSerializedBufferSize);
                }
                catch (IOException)
                {
                    Console.WriteLine("Timed out reading from tunnel");
                }
                var encapsulation = _tunnel.Unwrap(buffer);
                // this will be slow because it's blocking
                if (!connectionMap.TryGetValue(encapsulation.connectionId, out var connection))
                {
                    // create new connection to the source
                    var sourceClient = new TcpClient();
                    await sourceClient.ConnectAsync(sourceAddress, sourcePort);
                    Console.WriteLine("Connected to source at {0}:{1}", sourceAddress, sourcePort);

                    connection = new Connection
                    {
                        connectionId = encapsulation.connectionId,
                        client = sourceClient
                    };

                    connectionMap.TryAdd(encapsulation.connectionId, connection);
                }

                var sourceStream = connection.client.GetStream();
                try
                {
                    Console.WriteLine("Agent waiting to write request to source");
                    await sourceStream.WriteAsync(encapsulation.data, 0, encapsulation.data.Length);
                }
                catch (IOException e)
                {
                    Console.WriteLine("Caught IOException writing to source {0}", e);
                    continue;
                }

                try
                {
                    await _tunnel.WriteIntoTunnel(connection.client, encapsulation.connectionId);
                }
                catch (IOException e)
                {
                    Console.Error.WriteLine("Caught IOException writing to tunnel {0}", e);
                }
            }
        }
    }   
}