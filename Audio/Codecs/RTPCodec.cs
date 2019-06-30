using Frostbyte.Handlers;
using System;
using System.Buffers.Binary;

namespace Frostbyte.Audio.Codecs
{
    public sealed class RTPCodec
    {
        public const int HeaderSize = 12;

        private const byte RtpVersion = 0x78;
        private const byte RtpNoExtension = 0x80;

        public void EncodeHeader(ushort sequence, uint timestamp, uint ssrc, Span<byte> target)
        {
            if (target.Length < HeaderSize)
            {
                LogHandler<RTPCodec>.Log.Error("Header buffer is too short.");
                return;
            }

            target[0] = RtpNoExtension;
            target[1] = RtpVersion;

            // Write data big endian
            BinaryPrimitives.WriteUInt16BigEndian(target.Slice(2), sequence);  // header + magic
            BinaryPrimitives.WriteUInt32BigEndian(target.Slice(4), timestamp); // header + magic + sizeof(sequence)
            BinaryPrimitives.WriteUInt32BigEndian(target.Slice(8), ssrc);      // header + magic + sizeof(sequence) + sizeof(timestamp)
        }
    }
}