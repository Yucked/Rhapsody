using System;

namespace Frostbyte.Entities.Infos
{
    public struct VoiceInfo
    {
        public string SessionId { get; set; }
        public string Token { get; set; }
        public uint Ssrc { get; set; }
        public ReadOnlyMemory<byte> Key { get; set; }
    }
}