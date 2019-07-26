using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using Frostbyte.Factories;

namespace Frostbyte.Audio
{
    public sealed class AudioStream : Stream
    {
        public double Volume { get; set; } = 1.0;

        public ConcurrentQueue<AudioPacket> Packets { get; }

        /// <inheritdoc />
        public override bool CanRead
            => false;

        /// <inheritdoc />
        public override bool CanSeek
            => false;

        /// <inheritdoc />
        public override bool CanWrite
            => true;

        /// <inheritdoc />
        public override long Length { get; }

        /// <inheritdoc />
        public override long Position { get; set; }

        private readonly byte[] _pcmBuffer;

        private readonly int _pcmBufferDuration;
        private readonly Memory<byte> _pcmMemory;
        private ReadOnlyMemory<byte> _key;
        private int _pcmBufferLength;

        private uint _ssrc;

        public AudioStream(int bufferDuration)
        {
            _pcmBufferDuration = bufferDuration;
            _pcmBuffer = new byte[AudioHelper.GetSampleSize(bufferDuration)];
            _pcmMemory = _pcmBuffer.AsMemory();
            _pcmBufferLength = 0;
            Packets = new ConcurrentQueue<AudioPacket>();
        }

        /// <inheritdoc />
        public override void Flush()
        {
            var pcm = _pcmMemory.Span;
            AudioHelper.ZeroFill(pcm.Slice(_pcmBufferLength));
            var pcm16 = MemoryMarshal.Cast<byte, short>(pcm);

            if (Volume != 1)
                for (var i = 0; i < pcm16.Length; i++)
                    pcm16[i] = (short) (pcm16[i] * Volume);

            var packet = new byte[pcm.Length];
            var packetMemory = packet.AsMemory();
            if (!AudioHelper.TryPrepareAudioPacket(pcm, ref packetMemory, _ssrc, _key))
            {
                LogFactory.Error<AudioStream>("Failed to encrypt audio packet when flushing stream.");
                return;
            }

            Packets.Enqueue(new AudioPacket(packetMemory, _pcmBufferDuration));
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
            => 0;

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
            => 0;

        /// <inheritdoc />
        public override void SetLength(long value)
        {
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (_pcmBuffer)
            {
                var remaining = count;
                var buffSpan = buffer.AsSpan()
                    .Slice(offset, count);
                var pcmSpan = _pcmMemory.Span;

                while (remaining > 0)
                {
                    var len = Math.Min(pcmSpan.Length - _pcmBufferLength, remaining);

                    var tgt = pcmSpan.Slice(_pcmBufferLength);
                    var src = buffSpan.Slice(0, len);

                    src.CopyTo(tgt);
                    _pcmBufferLength += len;
                    remaining -= len;
                    buffSpan = buffSpan.Slice(len);

                    if (_pcmBufferLength != _pcmBuffer.Length) continue;
                    var pcm16 = MemoryMarshal.Cast<byte, short>(pcmSpan);

                    if (Volume != 1.0)
                        for (var i = 0; i < pcm16.Length; i++)
                            pcm16[i] = (short) (pcm16[i] * Volume);

                    _pcmBufferLength = 0;
                    var packet = new byte[pcmSpan.Length];
                    var packetMemory = packet.AsMemory();
                    if (!AudioHelper.TryPrepareAudioPacket(pcmSpan, ref packetMemory, _ssrc, _key))
                    {
                        LogFactory.Error<AudioStream>("Failed to encrypt audio packet when writing to stream.");
                        return;
                    }

                    Packets.Enqueue(new AudioPacket(packetMemory, _pcmBufferDuration));
                }
            }
        }

        public void SetSsrc(uint ssrc)
        {
            _ssrc = ssrc;
        }

        public void SetKey(int[] key)
        {
            _key = key.ConvertToByte();
        }
    }
}