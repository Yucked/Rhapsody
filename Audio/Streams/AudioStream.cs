using System;
using System.IO;

namespace Frostbyte.Audio.Streams
{
    public abstract class AudioStream : BaseStream
    {
        public override bool CanWrite
            => true;

        public override int Read(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();
    }
}