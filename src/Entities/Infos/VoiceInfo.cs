using System;

namespace Frostbyte.Entities.Infos
{
    public struct VoiceInfo
    {
        public uint Ssrc { get; set; }
        public uint Timestamp { get; set; }
        public ushort Sequence { get; set; }
        public ReadOnlyMemory<byte> Key { get; set; }
    }
}