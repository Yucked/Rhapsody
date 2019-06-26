using Frostbyte.Extensions;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Frostbyte.Audio.Streams
{
    public sealed class OutputStream : AudioStream
    {
        public readonly byte[] _secret;
        private readonly UdpClient _udp;

        public OutputStream(UdpClient udp, byte[] secret)
        {
            _udp = udp;
            _secret = secret;
        }

        public override void WriteHeader(ushort seq, uint timestamp, bool missed)
        {
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _udp.SendAsync(buffer);
        }
    }
}