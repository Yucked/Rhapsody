using System;

namespace Frostbyte.Entities.Enums
{
    [Flags]
    public enum OpusErrorType
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