using System.ComponentModel;
using System.Data;
using System.Net.Sockets;
using System.Text;
using Console = System.Console;

namespace tunnel;

public class Connection
{
    public string connectionId { get; set; }
    public TcpClient client { get; set; }
}

public class Tunnel(TcpClient tunnelClient)
{
    private readonly List<Connection> _connections = new List<Connection>();

    public string AddConnection(TcpClient client)
    {
        var connectionId = CreateConnectionId();
        Console.WriteLine("Adding connection {0}", connectionId);
        _connections.Add(new Connection
        {
            client = client,
            connectionId = connectionId
        });

        return connectionId;
    }

    // 1024 byte buffer plus 6 char for connectionid
    public static readonly int MaxRawBufferSize = 1024;
    static readonly int PrefixSize = 6;
    private static readonly string PrefixControl = "TUNN";
    public static readonly int MaxSerializedBufferSize = MaxRawBufferSize + PrefixSize + PrefixControl.Length;
    public static readonly int ReadTimeoutMs = 1000;

    private static string CreateConnectionId()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        StringBuilder result = new StringBuilder(PrefixSize);
        Random random = new Random();

        for (int i = 0; i < PrefixSize ; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }

        return PrefixControl + result;
    }
    
    public static byte[] Wrap(Encapsulation encapsulation)
    {
        byte[] prefix = Encoding.UTF8.GetBytes(encapsulation.connectionId);
        Console.WriteLine("Try to wrap data with length {0}", encapsulation.data.Length);
        // probably really bad performance
        byte[] newData = new byte[MaxSerializedBufferSize];
        prefix.CopyTo(newData, 0);
        encapsulation.data.CopyTo(newData, prefix.Length);
        
        return newData;
    }

    public Encapsulation Unwrap(byte[] wrappedData)
    {
        var fullPrefix = Encoding.UTF8.GetString(wrappedData.Take(PrefixSize + PrefixControl.Length).ToArray());
        if (fullPrefix.Substring(0, 4) != PrefixControl)
        {
            Console.WriteLine(Encoding.UTF8.GetString(wrappedData));
            throw new Exception("Unwrap found invalid control prefix");
        }
        
        return new Encapsulation
        {
            connectionId = fullPrefix,
            data = wrappedData.Take(Range.StartAt(PrefixSize)).ToArray()
        };
    }

    public async Task WriteIntoTunnel(TcpClient source, string connectionId)
    {
        int i = -1;
        var tunnelStream = tunnelClient.GetStream();
        // TunnelStream should wait forever..
        // tunnelStream.ReadTimeout(ReadTimeoutMs);
        
        var sourceStream = source.GetStream();
        sourceStream.ReadTimeout = ReadTimeoutMs;
        
        while (i != 0)
        {
            byte[] buffer = new byte[MaxRawBufferSize];
            Console.WriteLine("WriteIntoTunnel waiting to read source stream");
            try
            {
                i = sourceStream.Read(buffer, 0, MaxRawBufferSize);
            } catch (IOException)
            {
                Console.WriteLine("WriteIntoTunnel Timed out reading from source");
                break;
            }

            buffer = Wrap(new Encapsulation { data = buffer, connectionId = connectionId });
            Console.WriteLine("WriteIntoTunnel waiting to write to tunnel");
            await tunnelStream.WriteAsync(buffer, 0, buffer.Length);
            Console.WriteLine("wrote {0} bytes to tunnel for {1}", buffer.Length, connectionId);
            // if the last message was smaller than max buffer we can assume that its done
        }
    }

    public async Task WriteFromTunnel()
    {
        int i = -1;
        var tunnelStream = tunnelClient.GetStream();
        tunnelStream.ReadTimeout = ReadTimeoutMs;
        while (i != 0)
        {
            byte[] buffer = new byte[MaxSerializedBufferSize];
            Console.WriteLine("WriteFromTunnel waiting to read from tunnel");
            try
            {
                i = tunnelStream.Read(buffer, 0, buffer.Length);
            }
            catch (IOException)
            {
                Console.WriteLine("WriteFromTunnel timed out reading from tunnel");
                break;
            }
            try 
            var encapsulation = Unwrap(buffer);

            var connection = GetConnection(encapsulation.connectionId);
            if (connection == null)
            {
                Console.Error.WriteLine("Cant find connection {0}", encapsulation.connectionId);
                break;
            }
            else
            {
                
                Console.WriteLine("Found connection {0}", encapsulation.connectionId);
                buffer = TrimEnd(encapsulation.data);
                if (!connection.client.Connected)
                {
                    Console.WriteLine("write from tunnel - destination is disconnected");
                    continue;
                }
                var clientStream = connection.client.GetStream();
                // set read timeout
                clientStream.ReadTimeout = ReadTimeoutMs;
                Console.WriteLine("WriteFromTunnel waiting to write to client");
                await clientStream.WriteAsync(buffer, 0, buffer.Length);
                Console.WriteLine("Wrote {0} bytes to connection {1}", encapsulation.data.Length, encapsulation.connectionId);
                // if the last message was smaller than max buffer we can assume that its done
            }
        }
    }

    private Connection? GetConnection(string connectionId)
    {
        return _connections.Find(connection => connection.connectionId == connectionId);
    } 
    
    private static byte[] TrimEnd(byte[] array)
    {
        int lastIndex = Array.FindLastIndex(array, b => b != 0);
        Array.Resize(ref array, lastIndex + 1);
        return array;
    }
}