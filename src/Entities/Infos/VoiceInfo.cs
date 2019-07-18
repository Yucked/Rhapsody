using System;

namespace Frostbyte.Entities.Infos
{
    public struct VoiceInfo
    {
        public uint Ssrc { get; set; }
        public ReadOnlyMemory<byte> Key { get; set; }
    }
}