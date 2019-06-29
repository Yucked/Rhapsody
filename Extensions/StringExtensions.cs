using Frostbyte.Sources;
using System;
using System.Text;

namespace Frostbyte.Extensions
{
    public static class StringExtensions
    {
        public static string WithPath(this string str, string path)
        {
            return $"{str}/{path}";
        }

        public static string WithParameter(this string str, string key, string value)
        {
            return str.Contains("?") ?
                str += $"&{key}={value}" :
                str += $"?{key}={value}";
        }

        public static Uri ToUrl(this string str)
        {
            return new Uri(str);
        }

        public static (string Name, Type SourceType) GetSourceInfo(this string prefix)
        {
            prefix = prefix.Replace("search", "");
            return prefix switch
            {
                "apm"   => ("AppleMusic", typeof(AppleMusicSource)),
                "am"    => ("Audiomack", typeof(AudiomackSource)),
                "bc"    => ("BandCamp", typeof(BandCampSource)),
                "ht"    => ("HTTP", typeof(HttpSource)),
                "lcl"   => ("Local", typeof(LocalSource)),
                "mxc"   => ("MixCloud", typeof(MixCloudSource)),
                "mx"    => ("Mixer", typeof(MixerSource)),
                "mb"    => ("MusicBed", typeof(MusicBedSource)),
                "sc"    => ("SoundCloud", typeof(SoundCloudSource)),
                "tw"    => ("Twitch", typeof(TwitchSource)),
                "vm"    => ("Vimeo", typeof(VimeoSource)),
                "yt"    => ("YouTube", typeof(YouTubeSource)),
                _       => ("Unknown", null)
            };
        }

        public static (string Provider, string Title, string Url) DecodeHash(this string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                return default;

            var base64 = Convert.FromBase64String(hash);
            var raw = Encoding.UTF8.GetString(base64);
            var split = raw.Split(':');
            return (split[0], split[2], split[3]);
        }
    }
}