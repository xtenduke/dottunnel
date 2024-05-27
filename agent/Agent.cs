using System.Net.Sockets;

namespace agent;

using tunnel;

// Source address is the source we are connecting to
// Destination is the server 
public class Agent(String sourceAddress, Int32 sourcePort, String proxyAddress, Int32 proxyPort)
{
    // Open a connection to the destination
    public void Run()
    {
        // keep open
        while (true)
        {
            // when server accepts a connection
            var proxyConnection = ConnectionManager.EstablishConnection(proxyAddress, proxyPort);
            // connect out to the source
            var sourceConnection = ConnectionManager.EstablishConnection(sourceAddress, sourcePort);
            // swap buffers between
            ThreadPool.QueueUserWorkItem(delegate { HandleConnection(proxyConnection, sourceConnection); });
        }
    }

    private void HandleConnection(TcpClient proxyConnection, TcpClient sourceConnection)
    {
        var proxyStream = proxyConnection.GetStream();
        var sourceStream = sourceConnection.GetStream();
        Task.Run(() => ConnectionManager.ForwardData(proxyStream, sourceStream));
        Task.Run(() => ConnectionManager.ForwardData(sourceStream, proxyStream));
    }
}