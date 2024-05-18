using System.ComponentModel;
using System.Data;
using System.Net.Sockets;
using System.Text;

namespace tunnel;

public class Connection
{
    public string connectionId { get; set; }
    public NetworkStream stream { get; set; }
}

public class Tunnel(NetworkStream tunnelStream)
{

    private List<Connection> _connections = new List<Connection>();

    public string AddConnection(NetworkStream stream)
    {
        string connectionId = CreateConnectionId();
        _connections.Add(new Connection
        {
            stream = stream,
            connectionId = connectionId
        });

        return connectionId;
    }

    // 1024 byte buffer plus 6 char for connectionid
    public static readonly int MaxSerializedBufferSize = 1024 + 6;
    static readonly int PrefixSize = 6;

    private static string CreateConnectionId()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        StringBuilder result = new StringBuilder(PrefixSize);
        Random random = new Random();

        for (int i = 0; i < PrefixSize ; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }

        return result.ToString();
    }
    
    private static byte[] Wrap(Encapsulation encapsulation)
    {
        byte[] prefix = Encoding.UTF8.GetBytes(encapsulation.connectionId);
        // probably really bad performance
        byte[] newData = new byte[prefix.Length + encapsulation.data.Length];
        prefix.CopyTo(newData, 0);
        encapsulation.data.CopyTo(newData, prefix.Length);
                
        return newData;
    }

    private Encapsulation Unwrap(byte[] wrappedData)
    {
        return new Encapsulation
        {
            connectionId = Encoding.UTF8.GetString(wrappedData.Take(PrefixSize).ToArray()),
            data = wrappedData.Take(Range.StartAt(PrefixSize)).ToArray()
        };
    }

    public void WriteIntoTunnel(NetworkStream source, string connectionId)
    {
        byte[] buffer = new byte[1024];
        int i = -1;
        while (i != 0)
        {
            i = source.Read(buffer, 0, buffer.Length);
            buffer = Wrap(new Encapsulation { data = buffer, connectionId = connectionId });
            buffer = TrimEnd(buffer);
            tunnelStream.Write(buffer, 0, buffer.Length);
            // if the last message was smaller than max buffer we can assume that its done
            if (buffer.Length != 1024)
            {
                break;
            }
        }
    }

    public void WriteFromTunnel()
    {
        byte[] buffer = new byte[1024];
        int i = -1;
        while (i != 0)
        {
            i = tunnelStream.Read(buffer, 0, buffer.Length);
            Encapsulation encapsulation = Unwrap(buffer);
            Connection? connection = GetConnection(encapsulation.connectionId);
            if (connection == null)
            {
                // remind me to look up normal exception types, this sorta fits
                throw new InvalidAsynchronousStateException();
            }
            else
            {
                buffer = TrimEnd(buffer);
                connection.stream.Write(buffer, 0, buffer.Length);
                // if the last message was smaller than max buffer we can assume that its done
                if (buffer.Length != 1024)
                {
                    break;
                }    
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