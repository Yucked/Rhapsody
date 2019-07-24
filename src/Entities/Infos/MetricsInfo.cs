namespace Frostbyte.Entities.Infos
{
    public struct MetricsInfo
    {
        public long Uptime { get; set; }
        public int PlayingPlayers { get; set; }
        public int ConnectedClients { get; set; }
        public int ConnectedPlayers { get; set; }
        public CpuInfo Cpu { get; set; }
        public MemoryInfo Memory { get; set; }
        public FrameInfo Frames { get; set; }
    }
}