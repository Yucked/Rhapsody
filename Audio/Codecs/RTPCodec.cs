using System;
using System.Buffers.Binary;
using Frostbyte.Handlers;

namespace Frostbyte.Audio.Codecs
{
    public sealed class RtpCodec
    {
        public const int HEADER_SIZE = 12;

        private const byte RTP_VERSION = 0x78;
        private const byte RTP_NO_EXTENSION = 0x80;

        public void EncodeHeader(ushort sequence, uint timestamp, uint ssrc, Span<byte> target)
        {
            if (target.Length < HEADER_SIZE)
            {
                LogHandler<RtpCodec>.Log.Error("Header buffer is too short.");
                return;
            }

            target[0] = RTP_NO_EXTENSION;
            target[1] = RTP_VERSION;

            // Write data big endian
            BinaryPrimitives.WriteUInt16BigEndian(target.Slice(2), sequence);  // header + magic
            BinaryPrimitives.WriteUInt32BigEndian(target.Slice(4), timestamp); // header + magic + sizeof(sequence)
            BinaryPrimitives.WriteUInt32BigEndian(target.Slice(8),
                ssrc); // header + magic + sizeof(sequence) + sizeof(timestamp)
        }
    }
}