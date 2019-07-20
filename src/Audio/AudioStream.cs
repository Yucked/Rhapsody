using System.IO;

namespace Frostbyte.Audio
{
    public sealed class AudioStream
    {
        private uint _timestamp;
        private ushort _sequence;
        private readonly Stream _sourceStream;

        public AudioStream(Stream sourceStream)
        {
            _sourceStream = sourceStream;
        }
    }
}