using Frostbyte.Entities.Discord;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Frostbyte.Extensions
{
    public static class UdpClientExtensions
    {
        public static Task ProcessVoiceReadyAsync(this UdpClient client, VoiceReadyPayload vrp)
        {
            var packet = new byte[70];
            packet[0] = (vrp.SSRC >> 24).TryCast<byte>();
            packet[1] = (vrp.SSRC >> 16).TryCast<byte>();
            packet[2] = (vrp.SSRC >> 8).TryCast<byte>();
            packet[3] = (vrp.SSRC >> 0).TryCast<byte>();
            return SendAsync(client, packet);
        }

        public static Task SendAsync(this UdpClient client, byte[] data)
        {
            return client.SendAsync(data, data.Length);
        }
    }
}