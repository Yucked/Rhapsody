using System.Net.Sockets;
using System.Threading.Tasks;

namespace Frostbyte.Extensions
{
    public static class UdpClientExtensions
    {
        public static Task SendDiscoveryAsync(this UdpClient client, uint ssrc)
        {
            var packet = new byte[70];
            packet[0] = (byte) (ssrc >> 24);
            packet[1] = (byte) (ssrc >> 16);
            packet[2] = (byte) (ssrc >> 8);
            packet[3] = (byte) (ssrc >> 0);
            return SendAsync(client, packet);
        }

        public static Task SendKeepAliveAsync(this UdpClient client, ref int keepAlive)
        {
            var value = keepAlive++;
            var packet = new byte[8];
            packet[0] = (byte) (value >> 0);
            packet[1] = (byte) (value >> 8);
            packet[2] = (byte) (value >> 16);
            packet[3] = (byte) (value >> 24);
            packet[4] = (byte) (value >> 32);
            packet[5] = (byte) (value >> 40);
            packet[6] = (byte) (value >> 48);
            packet[7] = (byte) (value >> 56);
            return SendAsync(client, packet);
        }

        public static Task SendAsync(this UdpClient client, byte[] data)
        {
            return client.SendAsync(data, data.Length);
        }
    }
}