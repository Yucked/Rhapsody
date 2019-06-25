namespace Frostbyte.Entities.Enums
{
    public enum VoiceOPType : byte
    {
        Identify                = 0,
        Ready                   = 2,
        Heartbeat               = 3,        
        SessionDescription      = 4,
        Speaking                = 5,
        Hello                   = 8,        
    }
}