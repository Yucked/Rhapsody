namespace Frostbyte.Entities.Enums
{
    public enum VoiceOpType : byte
    {
        Identify = 0,
        SelectProtocol = 1,
        Ready = 2,
        Heartbeat = 3,
        SessionDescription = 4,
        Speaking = 5,
        Hello = 8
    }
}