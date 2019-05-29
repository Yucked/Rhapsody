using System.Runtime.Serialization;

namespace Frostbyte.Enums
{
    public enum Operation
    {
        [EnumMember(Value = "VoiceUpdate")]
        VoiceUpdate = 11,

        [EnumMember(Value = "Play")]
        Play = 1,

        [EnumMember(Value = "Pause")]
        Pause = 2,

        [EnumMember(Value = "Stop")]
        Stop = 3,

        [EnumMember(Value = "Skip")]
        Skip = 4,

        [EnumMember(Value = "Seek")]
        Seek = 5,

        [EnumMember(Value = "Volume")]
        Volume = 6,

        [EnumMember(Value = "Destroy")]
        Destroy = 0,

        [EnumMember(Value = "Equalizer")]
        Equalizer = 7,

        [EnumMember(Value = "Stats")]
        Statistics = 12
    }
}