using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Packets
{
    public sealed class StatisticPacket : BasePacket
    {
        public StatisticPacket() : base(Operation.Statistics)
        {
        }

        [JsonPropertyName("pp")]
        public int PlayingPlayers { get; set; }

        [JsonPropertyName("cp")]
        public int ConnectedPlayers { get; set; }

        [JsonPropertyName("mem")]
        public MemoryEntity Memory { get; set; }

        [JsonPropertyName("cpu")]
        public CpuEntity Cpu { get; set; }

        [JsonPropertyName("upt")]
        public long Uptime { get; set; }

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
        [JsonPropertyName("usd")]
        public long Used { get; set; }

        [JsonPropertyName("alloc")]
        public long Allocated { get; set; }
    }

    public sealed class CpuEntity
    {
        [JsonPropertyName("crs")]
        public int Cores { get; set; }

        [JsonPropertyName("sysld")]
        public double SystemLoad { get; set; }

        [JsonPropertyName("pcld")]
        public double ProcessLoad { get; set; }
    }
}