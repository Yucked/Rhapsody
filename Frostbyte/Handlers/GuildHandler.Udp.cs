using System.Threading.Tasks;

namespace Frostbyte.Handlers
{
    public sealed partial class GuildHandler
    {
        private async Task HandleUdpAsync()
        {
            var packet = new byte[70];
            packet[0] = (byte)(VoiceReadyPayload.SSRC >> 24);
            packet[1] = (byte)(VoiceReadyPayload.SSRC >> 16);
            packet[2] = (byte)(VoiceReadyPayload.SSRC >> 8);
            packet[3] = (byte)(VoiceReadyPayload.SSRC >> 0);
            await SendUdpPacketAsync(packet).ConfigureAwait(false);
        }

        private Task SendUdpPacketAsync(byte[] data)
        {
            return UdpClient.SendAsync(data, data.Length);
        }
    }
}