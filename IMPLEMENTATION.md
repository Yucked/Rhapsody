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
        Voice       = 2048,
        Music       = 2049,
        LowLatency  = 2051
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
        Destroy         = 0,
        Ready           = 1,
        Play            = 2,
        Pause           = 3,
        Stop            = 4,
        Skip            = 5,
        Seek            = 6,
        Volume          = 7,
        Equalizer       = 8,
        VoiceUpdate     = 9,

        REST            = 11,
        Statistics      = 12,
        TrackUpdate     = 13,
        TrackErrored    = 14,
        TrackFinished   = 15
    }
```

#### `REST Response`
Searching for tracks will result in a response similar to this:
```json
{
    "LoadType": 2,
    "Playlist": {
      "Id": "RDQMfGoQ5l4eXrw",
      "Name": "Mix - Tf2",
      "Url": "https://www.youtube.com/list_ajax?style=json&action_get_list=1&list=RDQMfGoQ5l4eXrw",
      "Duration": 7321000,
      "ArtworkUrl": null
    },
    "Tracks": [
      {
        "Hash": null,
        "Id": "Ep1PTvHlMwg",
        "Url": "https://www.youtube.com/watch?v=Ep1PTvHlMwg",
        "Title": "SOMBRA VS SPY RAP BATTLE by JT Music (Overwatch vs TF2)",
        "Duration": 290000,
        "Position": 0,
        "CanStream": false,
        "ArtworkUrl": "https://img.youtube.com/vi/Ep1PTvHlMwg/maxresdefault.jpg",
        "Author": {
          "Name": "JT Music",
          "Url": null,
          "AvatarUrl": null
        }
      }
    ]
  }
```

`LoadType` is an `enum`. It matches Lavalink's style. Since the result above loaded a playlist, `PlaylistLoaded` was returned for `LoadType`. For the sake of space, I've removed rest of the search results but it should return about all the videos in a YouTube playlist.

```cs
    public enum LoadType
    {
        TrackLoaded     = 1,    
        PlaylistLoaded  = 2,
        SearchResult    = 3,
        NoMatches       = 4,
        LoadFailed      = 5
    }
```
