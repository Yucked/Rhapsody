namespace Frostbyte.Entities.Payloads
{
    public sealed class VoiceServerPayload : BasePayload
    {
        public string SessionId { get; set; }
        public string Token { get; set; }
        public string Endpoint { get; set; }
    }
}