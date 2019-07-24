using Frostbyte.Entities.Enums;
using Frostbyte.Entities.Infos;

namespace Frostbyte.Entities.EventArgs
{
    public class PlayerEvent : BaseEvent
    {
        public ulong GuildId { get; }

        public TrackInfo Track { get; }

        public PlayerEvent(EventType eventType, ulong guildId, TrackInfo track) : base(eventType)
        {
            GuildId = guildId;
            Track = track;
        }
    }

    public sealed class TrackEndEvent : PlayerEvent
    {
        public TrackEndReason Reason { get; set; }

        public TrackEndEvent(EventType eventType, ulong guildId, TrackInfo track)
            : base(eventType, guildId, track)
        {
        }
    }
}