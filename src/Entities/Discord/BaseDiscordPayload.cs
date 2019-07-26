using System.Text.Json.Serialization;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities.Discord
{
    public struct BaseDiscordPayload<T>
    {
        [JsonPropertyName("op")]
        public VoiceOpType Op { get; set; }

        [JsonPropertyName("d")]
        public T Data { get; set; }

        public BaseDiscordPayload(T data)
        {
            Data = data;
            Op = data switch
            {
                IdentifyData _  => VoiceOpType.Identify,
                ResumeData _    => VoiceOpType.Resume,
                SpeakingData _  => VoiceOpType.Speaking,
                SelectPayload _ => VoiceOpType.SelectProtocol,
                ReadyData _     => VoiceOpType.Ready,
                HelloData _     => VoiceOpType.Hello,
                long _          => VoiceOpType.Heartbeat,
                _               => default
            };
        }
    }
}