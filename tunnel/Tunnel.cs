using System.Net.Sockets;

namespace tunnel;

using System.Text;

public class Tunnel
{
    private const int ChunkSize = 2048;
    private static readonly byte[] ControlStartBytes = new byte[] { 0x57, 0x52, 0x41, 0x50, 0x43, 0x4f, 0x4e, 0x54, 0x52, 0x4f, 0x4c, 0x44, 0x41, 0x54, 0x41, 0x53, 0x54, 0x41, 0x52, 0x54, 0x30 };
    private static readonly byte[] ControlEndBytes = new byte[] { 0x57, 0x52, 0x41, 0x50, 0x43, 0x4f, 0x4e, 0x54, 0x52, 0x4f, 0x4c, 0x44, 0x41, 0x54, 0x41, 0x45, 0x4e, 0x44 };
    private const int ConnectionIdByteSize = 6;
    private static readonly int WrappedDataSize = ChunkSize + ControlStartBytes.Length + ControlEndBytes.Length + ConnectionIdByteSize;

    // Wrap a 2048 byte chunk in control data
    private byte[] Wrap(byte[] data, string connectionId)
    {
        // what happens if we don't have enough data to fill the buffer?
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
        return newData;
    }

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