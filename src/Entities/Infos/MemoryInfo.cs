namespace Frostbyte.Entities.Infos
{
    public readonly struct MemoryInfo
    {
        public long Used { get; }
        public long Allocated { get; }

        public MemoryInfo(long used, long allocated)
        {
            Used = used;
            Allocated = allocated;
        }
    }
}