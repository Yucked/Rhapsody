using System;
using System.Buffers.Binary;

namespace Frostbyte.Audio.Codecs
{
    public sealed class RTPCodec
    {
        public const int HeaderSize = 12;

        private const byte RtpNoExtension = 0x80;
        private const byte RtpVersion = 0x78;

        public void EncodeHeader(ushort sequence, uint timestamp, uint ssrc, Span<byte> target)
        {
            if (target.Length < HeaderSize)
                throw new ArgumentException("Header buffer is too short.", nameof(target));

            target[0] = RtpNoExtension;
            target[1] = RtpVersion;

            // Write data big endian
            BinaryPrimitives.WriteUInt16BigEndian(target.Slice(2), sequence);  // header + magic
            BinaryPrimitives.WriteUInt32BigEndian(target.Slice(4), timestamp); // header + magic + sizeof(sequence)
            BinaryPrimitives.WriteUInt32BigEndian(target.Slice(8), ssrc);      // header + magic + sizeof(sequence) + sizeof(timestamp)
        }

        public int CalculatePacketSize(int encryptedLength)
        {
            return HeaderSize + encryptedLength;
        }
    }
}