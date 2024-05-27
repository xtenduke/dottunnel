namespace server;
using System;
public class Program
{
    private static readonly String address = "127.0.0.1";
    // Listen to incoming connections from the Client/Agent
    private static readonly Int32 TunnelPort = 3001;
    
    // Listen to incoming connections to serve to users
    private static readonly Int32 UserPort = 8080;

    public static void Main()
    {
        Server server = new(address, TunnelPort, UserPort);

        server.Run();
    }
}