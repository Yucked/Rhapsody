namespace Frostbyte.Entities.Infos
{
    public readonly struct CpuInfo
    {
        public int Cores { get; }
        public double SystemLoad { get; }
        public double ProcessLoad { get; }

        public CpuInfo(int cores, double systemLoad, double processLoad)
        {
            Cores = cores;
            SystemLoad = systemLoad;
            ProcessLoad = processLoad;
        }
    }
}