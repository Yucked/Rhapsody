namespace Frostbyte.Entities.Infos
{
    public readonly struct FrameInfo
    {
        public int Sent { get; }
        public int Null { get; }

        public FrameInfo(int sent, int nulled)
        {
            Sent = sent;
            Null = nulled;
        }
    }
}