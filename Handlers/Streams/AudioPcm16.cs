using CSCore;
using CSCore.Streams.SampleConverter;
using System;

namespace Frostbyte.Handlers.Streams
{
    public sealed class AudioPcm16 : SampleToPcm16
    {
        private float BASE_VOLUME = 0.79f;

        public float Volume
        {
            get => BASE_VOLUME / 0.79f;
            set => BASE_VOLUME = value * 0.79f;
        }

        public AudioPcm16(ISampleSource source) : base(source)
        {

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Buffer = Buffer.CheckBuffer(count / 2);

            var read = Source.Read(Buffer, 0, count / 2);
            var bufferOffset = offset;

            for (int i = 0; i < read; i++)
            {
                int value = (int)(Buffer[i] * BASE_VOLUME * short.MaxValue);
                var bytes = BitConverter.GetBytes(value);

                buffer[bufferOffset++] = bytes[0];
                buffer[bufferOffset++] = bytes[1];
            }

            return read * 2;
        }
    }
}