using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace Frostbyte.Entities
{
    public sealed class StatisticsEntity : BaseOP
    {
        public StatisticsEntity() : base("stats")
        {
        }

        [JsonProperty("pp")]
        public int PlayingPlayers { get; set; }

        [JsonProperty("cp")]
        public int ConnectedPlayers { get; set; }

        [JsonProperty("mem")]
        public MemoryEntity Memory { get; set; }

        [JsonProperty("cpu")]
        public CpuEntity Cpu { get; set; }

        [JsonProperty("upt")]
        public TimeSpan Uptime { get; set; }

        public StatisticsEntity Populate(Process process)
        {
            Memory.Populate(process);
            Cpu.Populate(process);

            return this;
        }
    }

    public sealed class MemoryEntity
    {
        [JsonProperty("u")]
        public long Used { get; set; }

        [JsonProperty("alloc")]
        public long Allocated { get; set; }

        public MemoryEntity Populate(Process process)
        {
            Allocated = process.VirtualMemorySize64;
            Used = GC.GetTotalMemory(true);
            return this;
        }
    }

    public sealed class CpuEntity
    {
        [JsonProperty("crs")]
        public int Cores { get; set; }

        [JsonProperty("sysld")]
        public double SystemLoad { get; set; }

        [JsonProperty("pcld")]
        public double ProcessLoad { get; set; }

        public CpuEntity Populate(Process process)
        {
            Cores = Environment.ProcessorCount;

            return this;
        }
    }
}