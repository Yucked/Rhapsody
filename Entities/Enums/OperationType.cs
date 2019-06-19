namespace Frostbyte.Entities.Enums
{
    public enum OperationType
    {
        Destroy = 0,
        Play = 1,
        Pause = 2,
        Stop = 3,
        Skip = 4,
        Seek = 5,
        Volume = 6,
        Equalizer = 7,
        VoiceUpdate = 8,

        REST = 11,
        Statistics = 12,
        TrackUpdate = 13,
        TrackErrored = 14,
        TrackFinished = 15
    }
}