using CSCore;
using System;

namespace Frostbyte.Handlers.Streams
{
    public sealed class EqualizerSampleSource : ISampleSource
    {
        public bool CanSeek
            => throw new NotImplementedException();

        public WaveFormat WaveFormat
            => throw new NotImplementedException();

        public long Position { get; set; }

        public long Length
            => throw new NotImplementedException();

        private readonly float[] _bands;
        private readonly ISampleSource _source;        

        public EqualizerSampleSource(ISampleSource source, float[] bands = default)
        {
            _bands = bands;
            _source = source;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public int Read(float[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}