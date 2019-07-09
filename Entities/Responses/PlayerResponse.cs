using Frostbyte.Entities.Audio;

namespace Frostbyte.Entities.Responses
{
    public struct PlayerResponse
    {
        public ulong GuildId { get; set; }
        public AudioTrack Track { get; set; }
    }
}