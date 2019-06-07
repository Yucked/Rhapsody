using System.Text.RegularExpressions;

namespace Frostbyte.Extensions
{
    public static class StringExtensions
    {
        public static string ReplaceArgument(this string str, string data)
        {
            return str.Replace("{0}", data);
        }

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

        public static string GetSourceFromPrefix(this string str)
        {
            str = str.Replace("search", "");
            return str switch
            {
                "yt" => "YouTube",
                "sc" => "SoundCloud",
                "lcl" => "Local"
            };
        }
    }
}