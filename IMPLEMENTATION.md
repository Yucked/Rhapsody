# 🕹️ `Write your own library 101:`

## > Configuration Explained.
This is a sample configuration that Frostbyte generates on startup. Everytime you change anything you'll have to restart the server.
`Host`, `Port`, `Password` are along with `Sources` are self explanatory.

```json
{
  "Port": 6666,
  "Host": "127.0.0.1",
  "Password": "frostbyte",
  "LogLevel": 0,
  "Sources": {
    "EnableAppleMusic": false,
    "EnableAudiomack": false,
    "EnableBandCamp": false,
    "EnableHttp": true,
    "EnableLocal": true,
    "EnableMixCloud": false,
    "EnableMixer": false,
    "EnableMusicBed": false,
    "EnableSoundCloud": true,
    "EnableTwitch": false,
    "EnableVimeo": false,
    "EnableYouTube": true
  },
  "ReconnectInterval": 5000,
  "MaxConnectionRetries": 10,
  "VoiceSettings": 2049
}
```

`LogLevel` is an `enum`:
```cs
    public enum LogLevel
    {
        Critical        = 6,
        Debug           = 2,
        Error           = 5,
        Information     = 3,
        None            = 0,
        Trace           = 1,
        Warning         = 4
    }
 ```
 
 `ReconnectInterval` and `MaxConnectionRetries` are for when server loses connection to internet. It will retry for x times before
 discarding/disposing everything. For infinite retries set `MaxConnectionRetries` to `-1`.
 
 `VoiceSettings` is an `enum` and specifes settings for OPUS:
 ```cs
public enum VoiceSettings
{
    Voice = 2048,
    Music = 2049,
    LowLatency = 2051
}
```

---

## > Handling Responses.
Frostbyte returns the same base response for REST and WebSocket.

`IsSuccess`: Whether the response was successful in general.

`Reason:` Reason for why the response failed. If `IsSuccess` is true then reason is `Success`.

`Operation:` Is an enum which shows different responses. Perhaps returned as an `int`.

`Object` Any additional object. For REST, it will be a tracks response. For WebSocket, an event and so forth.

```json
{
  "IsSuccess": false,
  "Reason": null,
  "Operation": 0,
  "Object": null
}
```


```cs
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
```
