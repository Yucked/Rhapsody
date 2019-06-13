using System;
using System.Diagnostics;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Packets
{
    public sealed class StatisticPacket : BasePacket
    {
        public StatisticPacket() : base(OperationType.Statistics)
        {
        }

        public long Uptime { get; set; }
        public CpuEntity Cpu { get; set; }
        public int PlayingPlayers { get; set; }
        public MemoryEntity Memory { get; set; }
        public int ConnectedClients { get; set; }
        public int ConnectedPlayers { get; set; }

        public StatisticPacket Populate(Process process)
        {
            Memory = new MemoryEntity
            {
                Used = GC.GetTotalMemory(true),
                Allocated = process.VirtualMemorySize64
            };

            Cpu = new CpuEntity
            {
                Cores = Environment.ProcessorCount
            };

            return this;
        }
    }

    public sealed class MemoryEntity
    {
        public long Used { get; set; }
        public long Allocated { get; set; }
    }

    public sealed class CpuEntity
    {
        public int Cores { get; set; }
        public double SystemLoad { get; set; }
        public double ProcessLoad { get; set; }
    }
}