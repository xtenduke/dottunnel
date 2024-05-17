using System.Text;

namespace agent;

using System.Net.Sockets;

// Source address is the source we are connecting to
// Destination is the server 
public class Agent(String sourceAddress, Int32 sourcePort, String proxyAddress, Int32 proxyPort)
{
    // Open a connection to the destination
    public void Run()
    {
        // Connect to the proxy
        TcpClient proxyClient = new TcpClient();
        proxyClient.Connect(proxyAddress, proxyPort);
        NetworkStream proxyStream = proxyClient.GetStream();
        Console.WriteLine("Connected to proxy server at {0}:{1}", proxyAddress, proxyPort);
        
        // Connect to the source
        TcpClient sourceClient = new TcpClient();
        sourceClient.Connect(sourceAddress, sourcePort);
        NetworkStream sourceStream = sourceClient.GetStream();
        Console.WriteLine("Connected to source at {0}:{1}", sourceAddress, sourcePort);

        while (true)
        {
            Console.WriteLine("Try to read from proxy and write to source");
            CrossStreams(proxyStream, sourceStream);
            Console.WriteLine("Agent wrote proxy to source");
            CrossStreams(sourceStream, proxyStream);
            Console.WriteLine("Agent wrote source to proxy");
        }
    }   
    
    private void CrossStreams(NetworkStream source, NetworkStream destination)
    {
        // source.CopyTo(destination);
        Console.WriteLine("Writing stream");
        Byte[] buffer = new byte[1024];
        int i = -1;
        while (i != 0)
        {
            i = source.Read(buffer, 0, buffer.Length);
            buffer = TrimEnd(buffer); // trim trailing from buffer so server doesn't get confused
            Console.WriteLine("Read length was {0}", i);
            var message = Encoding.UTF8.GetString(buffer, 0, i);
            Console.WriteLine($"Message received: \"{message}\"");
            destination.Write(buffer, 0, buffer.Length);
            Console.WriteLine("Wrote buffer");
            if (buffer.Length != 1024)
            {
                break;
            }
        }
        Console.WriteLine("Done crossing");
    }
    
    public static byte[] TrimEnd(byte[] array)
    {
        int lastIndex = Array.FindLastIndex(array, b => b != 0);

        Array.Resize(ref array, lastIndex + 1);

        return array;
    }
}