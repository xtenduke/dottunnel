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
            var proxyStream = proxyConnection.GetStream();
            // connect out to the source
            var sourceConnection = ConnectionManager.EstablishConnection(sourceAddress, sourcePort);
            var sourceStream = sourceConnection.GetStream();
            // swap buffers between
            Task.Run(() => ConnectionManager.ForwardData(proxyStream, sourceStream));
            Task.Run(() => ConnectionManager.ForwardData(sourceStream, proxyStream));
            Thread.Sleep(1000);
        }
    }
}