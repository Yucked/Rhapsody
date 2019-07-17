using System;
using System.Buffers.Binary;
using Frostbyte.Factories;

namespace Frostbyte.AudioEngine.Codecs
{
    public sealed class RtpCodec
    {
        public const int HEADER_SIZE = 12;
        private const byte RTP_VERSION = 0x78;
        private const byte RTP_NO_EXTENSION = 0x80;

        public static bool TryEncodeHeader(ushort sequence, uint timestamp, uint ssrc, Span<byte> target)
        {
            if (target.Length < HEADER_SIZE)
            {
                LogFactory.Error<RtpCodec>("Header buffer is too short.");
                return false;
            }

            target[0] = RTP_NO_EXTENSION;
            target[1] = RTP_VERSION;

            BinaryPrimitives.WriteUInt16BigEndian(target.Slice(2), sequence);
            BinaryPrimitives.WriteUInt32BigEndian(target.Slice(4), timestamp);
            BinaryPrimitives.WriteUInt32BigEndian(target.Slice(8), ssrc);

            return true;
        }
    }
}