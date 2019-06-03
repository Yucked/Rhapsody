namespace Frostbyte.Entities
{
    public sealed class RatelimitPolicy
    {
        public bool IsEnabled { get; set; }
        public int PerSecond { get; set; }
        public int PerMinute { get; set; }
        public int PerHour { get; set; }
        public int PerDay { get; set; }
    }
}