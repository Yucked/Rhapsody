using System;
using System.Threading.Tasks;

namespace Frostbyte.Audio.Streams
{
    public sealed class RTPStream : AudioStream
    {
        private readonly SodiumStream _sodium;
        private readonly byte[] _header;
        protected readonly byte[] _buffer;
        private uint _ssrc;
        private ushort _nextSeq;
        private uint _nextTimestamp;
        private bool _hasHeader;

        public RTPStream(SodiumStream sodiumStream)
        {
            _sodium = sodiumStream;
        }


    }
}