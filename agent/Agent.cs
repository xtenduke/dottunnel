using System.Net.Sockets;

namespace agent;

using tunnel;

// Source address is the source we are connecting to
// Destination is the server 
public class Agent(string sourceAddress, int sourcePort, string proxyAddress, int proxyPort)
{
    // Open a connection to the destination
    public void Run()
    {
        // keep open
        while (true)
        {
            // when server accepts a connection
            var proxyConnection = ConnectionHelper.EstablishConnection(proxyAddress, proxyPort);
            // connect out to the source
            var sourceConnection = ConnectionHelper.EstablishConnection(sourceAddress, sourcePort);
            // swap buffers between
            ThreadPool.QueueUserWorkItem(delegate { HandleConnection(proxyConnection, sourceConnection); });
        }
    }

    private void HandleConnection(TcpClient proxyConnection, TcpClient sourceConnection)
    {
        var proxyStream = proxyConnection.GetStream();
        var sourceStream = sourceConnection.GetStream();
        Task.Run(() => ConnectionHelper.ForwardData(proxyStream, sourceStream));
        Task.Run(() => ConnectionHelper.ForwardData(sourceStream, proxyStream));
    }
}