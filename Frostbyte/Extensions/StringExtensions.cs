using System;
using System.Text.RegularExpressions;

namespace Frostbyte.Extensions
{
    public static class StringExtensions
    {
        private static Regex _reg;

        public static bool IsMatch(this string input, string pattern)
        {
            _reg ??= new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return _reg.IsMatch(input);
        }

        public static string ReplaceArgument(this string str, string data)
        {
            return str.Replace("{0}", data);
        }

        public static string GetSourceFromPrefix(this string str)
        {
            str = str.Replace("search", "");
            return str switch
            {
                "yt"    => "YouTube",
                "sc"    => "SoundCloud",
                "lcl"   => "Local"
            };
        }

        public static Regex ToRegex(this string str)
            => new Regex(str, RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }
}