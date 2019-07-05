using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Frostbyte.Audio
{
    public sealed class AudioStream : Stream
    {
        private readonly AudioEngine _engine;

        public AudioStream(AudioEngine engine, int bufferDuration = 20)
        {
            _engine = engine;
            BufferDuration = bufferDuration;
            Buffer = new byte[AudioHelper.GetSampleSize(bufferDuration)];
            BufferMemory = Buffer.AsMemory();
            BufferLength = 0;
        }

        public int Volume { get; private set; }
        public byte[] Buffer { get; }
        public int BufferLength { get; private set; }
        public int BufferDuration { get; }
        public Memory<byte> BufferMemory { get; }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (Buffer)
            {
                var remaining = count;
                var buffSpan = buffer.AsSpan().Slice(offset, count);
                var pcmSpan = BufferMemory.Span;

                while (remaining > 0)
                {
                    var len = Math.Min(pcmSpan.Length - BufferLength, remaining);

                    var tgt = pcmSpan.Slice(BufferLength);
                    var src = buffSpan.Slice(0, len);

                    src.CopyTo(tgt);
                    BufferLength += len;
                    remaining -= len;
                    buffSpan = buffSpan.Slice(len);

                    if (BufferLength != Buffer.Length)
                        continue;

                    var pcm16 = MemoryMarshal.Cast<byte, short>(pcmSpan);
                    if (Volume != 1)
                        for (var i = 0; i < pcm16.Length; i++)
                            pcm16[i] = (short) (pcm16[i] * Volume);

                    BufferLength = 0;
                    var packet = new byte[pcmSpan.Length];
                    var packetMemory = packet.AsMemory();

                    _engine.BuildAudioPacket(pcmSpan, ref packetMemory);
                    _engine.Packets.Enqueue(new AudioPacket
                    {
                        Bytes = packetMemory,
                        MillisecondDuration = BufferDuration
                    });
                }
            }
        }

        public override void Flush()
        {
            var pcm = BufferMemory.Span;
            AudioHelper.ZeroFill(pcm.Slice(BufferLength));
            var pcm16 = MemoryMarshal.Cast<byte, short>(pcm);

            if (Volume != 1)
                for (var i = 0; i < pcm16.Length; i++)
                    pcm16[i] = (short) (pcm16[i] * Volume);

            var packet = new byte[pcm.Length];
            var packetMemory = packet.AsMemory();

            lock (_engine)
            {
                _engine.BuildAudioPacket(pcm, ref packetMemory);
                _engine.Packets.Enqueue(new AudioPacket
                {
                    Bytes = packetMemory,
                    MillisecondDuration = BufferDuration
                });
            }
        }

        #region NOT NEEDED

        public override bool CanRead
            => false;

        public override bool CanSeek
            => false;

        public override bool CanWrite
            => true;

        public override long Length
            => default;

        public override long Position
        {
            get => default;
            set { }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return default;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return default;
        }

        public override void SetLength(long value)
        {
        }

        #endregion
    }
}