using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Frostbyte.Enums
{
    public enum Operation
    {
        [EnumMember(Value = "VoiceUpdate")]
        VoiceUpdate,

        [EnumMember(Value = "Play")]
        Play,

        [EnumMember(Value = "Pause")]
        Pause,

        [EnumMember(Value = "Stop")]
        Stop,

        [EnumMember(Value = "Skip")]
        Skip,

        [EnumMember(Value = "Seek")]
        Seek,

        [EnumMember(Value = "Volume")]
        Volume,

        [EnumMember(Value = "Destroy")]
        Destroy,

        [EnumMember(Value = "Equalizer")]
        Equalizer,

        [EnumMember(Value = "Stats")]
        Statistics
    }
}