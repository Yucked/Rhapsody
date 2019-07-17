namespace Frostbyte.Entities.Payloads
{
    public class PlayPayload : BasePayload
    {
        public string TrackId { get; }
        public long StartTime { get; }
        public long EndTime { get; }
    }
}