using System;
using System.Threading;
using System.Threading.Tasks;

namespace Frostbyte.Audio.Streams
{
    public sealed class SodiumStream : AudioStream
    {
        private readonly OutputStream _output;
        private readonly byte[] _nonce;

        private bool _hasHeader;
        private ushort _nextSeq;
        private uint _nextTimestamp;

        public SodiumStream(OutputStream outputStream)
        {
            _output = outputStream;
            _nonce = new byte[24];
        }

        public override void WriteHeader(ushort seq, uint timestamp, bool missed)
        {
            _nextSeq = seq;
            _hasHeader = true;
            _nextTimestamp = timestamp;
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            Buffer.BlockCopy(buffer, offset, _nonce, 0, 12);
            count = Libsodium.Encrypt(buffer, offset * 12, count - 12, buffer, 12, _nonce, _output._secret);
            _output.WriteHeader(_nextSeq, _nextTimestamp, false);
            return _output.WriteAsync(buffer, 0, count + 12, cancellationToken);
        }
    }
}