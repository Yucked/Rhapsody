<p align="center">
  <img src="https://i.imgur.com/9C1lvQA.png"/>
  <a href="https://discord.gg/ZJaVXK8"> </br> <img src="https://img.shields.io/badge/Discord-Support-%237289DA.svg?logo=discord&amp;style=for-the-badge&amp;logoWidth=20" /> </a> 
  <a href="https://ci.appveyor.com/project/Yucked/frostbyte"> <img src="https://img.shields.io/appveyor/ci/yucked/frostbyte.svg?color=1ac9ed&label=Cooking&logo=appveyor&style=for-the-badge&logoWidth=40" /> </a>
  
  <p align="center">
    Lavalink but this time it's better and provides a lot more options and also doesn't require JAVA. That's some good shit right there 
    fam 👌.
  </p> 
</p>

### ⚗ `Building`
Frostbyte targets .NET Core SDK Preview 6 and Runtime Preview 7. Both of these can be obtained below:

SDK: https://github.com/dotnet/core-setup#daily-builds

Runtime: https://github.com/dotnet/core/blob/master/daily-builds.md

###  ✒ `Planned / Underdevelopment Features`
- [ ] UDP Discord stuff and handling OP codes.
- [ ] Implement the following sources: BandCamp, MixCloud, Audiomack, MusicBed, Twitch, Vimeo, Mixer, HTTP files, Apple Music(?)
- [ ] Building streams from above sources.
- [ ] Ratelimiter for REST requests with IP restrictions.
- [ ] Downloading audio files.
- [ ] Compatible with Lavalink entities and Victoria.
- [ ] Providing user with several audio options.
- [x] HttpListener handling Websocket and REST requests.
- [x] Building Track entity from SoundCloud, YouTube and local source.
- [x] Detailed logging along with beautiful console output that was necessary.
- [x] Automatic configuration building and handling.
- [x] Other stuff that I probably forgot to mention.

###  ☃ `Sample Configuration`
```json
{
  "Port": 6666,
  "Host": "127.0.0.1",
  "Password": "frostbyte",
  "LogLevel": 0,
  "Sources": {
    "use_yt": true,
    "use_sc": true,
    "use_twi": false,
    "use_vim": false,
    "use_lcl": true
  },
  "RatelimitPolicy": {
    "IsEnabled": true,
    "PerSecond": 5,
    "PerMinute": 69,
    "PerHour": 420,
    "PerDay": 1447
  }
}
```

### 📷 `Screenshots Because They Matter`
<p align="center">
  <img src="https://cdn.discordapp.com/attachments/522441208740446218/584206737482186772/unknown.png"/>
</p>
