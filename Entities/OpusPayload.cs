namespace Frostbyte.Entities
{
    public struct OpusPayload
    {
        public byte[] Bytes { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
        public uint Timestamp { get; set; }
        public ushort Sequence { get; set; }
    }
}