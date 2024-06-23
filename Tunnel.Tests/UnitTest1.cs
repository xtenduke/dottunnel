namespace Tunnel.Tests;

public class UnitTest1
{
    private static readonly int PacketSize = 2048;
    
    [Fact]
    public void Tunnel_should_properly_wrap_data()
    {
        var data = GenerateRandomByteArray(PacketSize);
        var connectionId = "ottffs";
        var tunnel = new tunnel.Tunnel();
        var wrappedData = tunnel.Wrap(data, connectionId);
    }

    private static readonly Random Random = new Random();

    public static byte[] GenerateRandomByteArray(int sizeInBytes)
    {
        byte[] buffer = new byte[sizeInBytes];
        Random.NextBytes(buffer);
        return buffer;
    }
    
}