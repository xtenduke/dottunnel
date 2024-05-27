using System.Net.Sockets;

namespace tunnel;

public class ConnectionHelper
{
    public static async Task ForwardData(NetworkStream source, NetworkStream destination)
    {
        var buffer = new byte[4096];
        int bytesRead;
        var totalRead = 0;
        while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            totalRead += bytesRead;
            await destination.WriteAsync(buffer, 0, bytesRead);
            Console.WriteLine("Transferred {0} bytes", bytesRead);
        }
        
        // if we haven't transferred anything yet, keep connections open
        if (totalRead == 0)
        {
            Thread.Sleep(200);
            await ForwardData(source, destination);
        }
        else
        {
            source.Close();
            destination.Close();
            Console.WriteLine("Connections closed");   
        }
    }
    
    public static TcpClient EstablishConnection(string destination, int port)
    {
        try
        {
            var client = new TcpClient();
            client.Connect(destination, port);
            Console.WriteLine("Connected at {0}:{1}", destination, port);
            return client;    
        } catch (SocketException)
        {
            Console.WriteLine("Connection failed, backing off");
            Thread.Sleep(200);
            return EstablishConnection(destination, port);
        }
    }
}