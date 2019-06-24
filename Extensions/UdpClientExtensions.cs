using Frostbyte.Entities.Discord;
using Frostbyte.Entities.Enums;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Frostbyte.Extensions
{
    public static class UdpClientExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="ssrc"></param>
        /// <returns></returns>
        public static Task SendDiscoveryAsync(this UdpClient client, int ssrc)
        {
            var packet = new byte[70];
            packet[0] = (byte)(ssrc >> 24);
            packet[1] = (byte)(ssrc >> 16);
            packet[2] = (byte)(ssrc >> 8);
            packet[3] = (byte)(ssrc >> 0);

            return client.SendAsync(packet, packet.Length);
        }

        public static Task SendAsync(this UdpClient client, byte[] data)
        {
            return client.SendAsync(data, data.Length);
        }
    }
}