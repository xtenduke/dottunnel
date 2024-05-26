using agent;

public class Program
{
    private static readonly String ProxyAddress = "127.0.0.1";
    private static readonly Int32 ProxyPort = 3001;

    private static readonly String SourceAddress = "100.95.138.94";
    private static readonly Int32 SourcePort = 8112;

    static async Task Main()
    {
        Agent agent = new Agent(SourceAddress, SourcePort, ProxyAddress, ProxyPort);
        await agent.Run();
    }
}