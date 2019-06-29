using System;

namespace Frostbyte.Audio.Codecs.Enums
{
    [Flags]
    internal enum OpusError
    {
        Ok = 0,
        BadArgument = -1,
        BufferTooSmall = -2,
        InternalError = -3,
        InvalidPacket = -4,
        Unimplemented = -5,
        InvalidState = -6,
        AllocationFailure = -7
    }
}