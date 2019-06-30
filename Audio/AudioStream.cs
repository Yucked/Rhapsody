using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Frostbyte.Audio
{
    public sealed class AudioStream : Stream
    {
        #region NOT NEEDED
        public override bool CanRead
            => false;

        public override bool CanSeek
            => false;

        public override bool CanWrite
            => true;

        public override long Length
            => default;

        public override long Position {
            get => default;
            set => value = default;
        }

        public override int Read(byte[] buffer, int offset, int count) { return default; }

        public override long Seek(long offset, SeekOrigin origin) { return default; }

        public override void SetLength(long value) { }
        #endregion

        public int Volume { get; set; }

        public byte[] Buffer { get; }
        public int BufferLength { get; private set; }
        public int BufferDuration { get; }
        public Memory<byte> BufferMemory { get; }

        public AudioStream(AudioEngine engine, int bufferDuration = 20)
        {
            BufferDuration = bufferDuration;
            Buffer = new byte[AudioHelper.GetSampleSize(bufferDuration)];
            BufferMemory = Buffer.AsMemory();
            BufferLength = 0;
        }

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

                    if (BufferLength == Buffer.Length)
                    {
                        var pcm16 = MemoryMarshal.Cast<byte, short>(pcmSpan);


                        if (Volume != 1)
                        {
                            for (var i = 0; i < pcm16.Length; i++)
                                pcm16[i] = (short)(pcm16[i] * Volume);
                        }

                        BufferLength = 0;
                        var packet = new byte[pcmSpan.Length];
                        var packetMemory = packet.AsMemory();


                        //this.Connection.PreparePacket(pcmSpan, ref packetMemory);
                        //this.Connection.EnqueuePacket(new VoicePacket(packetMemory, this.PcmBufferDuration));
                    }
                }
            }
        }

        public override void Flush()
        {
            var pcm = BufferMemory.Span;
            AudioHelper.ZeroFill(pcm.Slice(BufferLength));
            var pcm16 = MemoryMarshal.Cast<byte, short>(pcm);

            if (Volume != 1)
            {
                for (var i = 0; i < pcm16.Length; i++)
                    pcm16[i] = (short)(pcm16[i] * Volume);
            }

            var packet = new byte[pcm.Length];
            var packetMemory = packet.AsMemory();

            //this.Connection.PreparePacket(pcm, ref packetMemory);
            //this.Connection.EnqueuePacket(new VoicePacket(packetMemory, this.PcmBufferDuration));
        }
    }
}