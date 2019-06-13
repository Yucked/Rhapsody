<p align="center">
  <img src="https://i.imgur.com/9C1lvQA.png"/>
  <a href="https://discord.gg/ZJaVXK8"> </br> <img src="https://img.shields.io/badge/Discord-Support-%237289DA.svg?logo=discord&amp;style=for-the-badge&amp;logoWidth=20" /> </a> 
  <a href="https://travis-ci.com/Yucked/Frostbyte"> <img src="https://img.shields.io/travis/com/yucked/frostbyte.svg?color=1ac9ed&label=Travis-CI&logo=appveyor&style=for-the-badge&logoWidth=40" /> </a>
  
  <p align="center">
    Lavalink but this time it's better and provides a lot more options and also doesn't require JAVA. That's some good shit right there 
    fam 👌.
  </p> 
</p>

> 

### ⚗ `Building`
You will need to obtain the latest SDK and Runtime for .NET Core in order to run Frostbyte. Please keep in mind Frostbyte is still under development.

SDK: https://github.com/dotnet/core-sdk/blob/master/README.md#installers-and-binaries

Runtime: https://github.com/dotnet/core-setup/blob/master/README.md#daily-builds

###  ✒ `Planned / Underdevelopment Features`
- [ ] UDP Discord stuff and handling OP codes.
- [ ] Implement the following sources: MixCloud, Audiomack, MusicBed, Twitch, Vimeo, Mixer, HTTP files, Apple Music(?)
- [ ] Building streams from above sources.
- [ ] Ratelimiter for REST requests with IP restrictions.
- [ ] Downloading audio files.
- [ ] Compatible with Lavalink entities and Victoria.
- [ ] Providing user with several audio options.
- [x] HttpListener handling Websocket and REST requests.
- [x] Sources that are done: SoundCloud, YouTube, LocalSource, BandCamp.
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
  }
}
```

### 📷 `Screenshots Because They Matter`
<p align="center">
  <img src="https://cdn.discordapp.com/attachments/522441208740446218/584206737482186772/unknown.png"/>
</p>
