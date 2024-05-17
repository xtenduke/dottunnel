using agent;

public class Program
{
    private static readonly String ProxyAddress = "127.0.0.1";
    private static readonly Int32 ProxyPort = 3001;

    private static readonly String SourceAddress = "192.168.1.254";
    private static readonly Int32 SourcePort = 80;

    private static void Main()
    {
        Agent agent = new Agent(SourceAddress, SourcePort, ProxyAddress, ProxyPort);
        agent.Run();
    }
}