using agent;

public class Program
{
    private const string ProxyAddress = "127.0.0.1";
    private const int ProxyPort = 3001;

    private const string SourceAddress = "100.95.138.94";
    private const  int SourcePort = 80;

    private static void Main()
    {
        var agent = new Agent(SourceAddress, SourcePort, ProxyAddress, ProxyPort);
        agent.Run();
    }
}