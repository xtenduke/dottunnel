namespace tunnel;

using System.Text;

// Current state
// SERVER starts and listens for connections from user and proxy only accepting one connection at a time
// When user and proxy connection is established, begin forwarding data from user connection to proxy and proxy back to the user
// Keep accepting connections from proxy and user one at a time, with max backlog of one
// Proposed state:
// Accept a few connections from the proxy, ~10 sounds like a nice number, and unlimited connections from users
// Begin forwarding connections from users, wrapping each 'frame' of n bytes that go through the proxy connection between server and agent
// Unwrap data at server and send to the correct incoming user connection

// unwrapping the data won't be as simple as it sounds...
// how do we make sure that we aren't getting chunks of data from different connections at different times?
// we will need to control flow at both sides somehow

public record UnwrappedPayload(byte[] Data, string ConnectionId);

public class Tunnel
{
    private const int ChunkSize = 2048;
    private static readonly byte[] ControlStartBytes = [0x57, 0x52, 0x41, 0x50, 0x43, 0x4f, 0x4e, 0x54, 0x52, 0x4f, 0x4c, 0x44, 0x41, 0x54, 0x41, 0x53, 0x54, 0x41, 0x52, 0x54, 0x30];
    private static readonly byte[] ControlEndBytes = [0x57, 0x52, 0x41, 0x50, 0x43, 0x4f, 0x4e, 0x54, 0x52, 0x4f, 0x4c, 0x44, 0x41, 0x54, 0x41, 0x45, 0x4e, 0x44];
    private const int ConnectionIdByteSize = 6;
    private static readonly int WrappedDataSize = ChunkSize + ControlStartBytes.Length + ControlEndBytes.Length + ConnectionIdByteSize;

    // Wrap a 2048 byte chunk in control data
    public static byte[] Wrap(byte[] data, string connectionId)
    {
        // what happens if we don't have enough data to fill the buffer?
        // fill it with zeros?
        if (data.Length != ChunkSize)
        {
            throw new Exception($"Wrap chunk size too large at {data.Length}, should be {ChunkSize}");
        }

        if (connectionId.Length != ConnectionIdByteSize)
        {
            throw new Exception(
                $"Wrap connection id size too large at {connectionId.Length}, should be {ConnectionIdByteSize}");
        }

        var newData = new byte[WrappedDataSize];
        Buffer.BlockCopy(ControlStartBytes, 0, newData, 0, ControlStartBytes.Length);
        Buffer.BlockCopy(data, 0, newData, 0, data.Length);
        var connectionIdBytes = Encoding.UTF8.GetBytes(connectionId);
        Buffer.BlockCopy(connectionIdBytes, 0, newData, 0, connectionIdBytes.Length);
        Buffer.BlockCopy(ControlEndBytes, 0, newData, 0, ControlEndBytes.Length);
        return newData;
    }

    // this probably wont work
    public static UnwrappedPayload Unwrap(byte[] wrappedData)
    {
        // find the first byte
    }

    // Using string data for ids
    public static string GetRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        StringBuilder result = new StringBuilder(length);
        Random random = new Random();

        for (int i = 0; i < length; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }

        return result.ToString();
    }
}