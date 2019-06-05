﻿using System;
using System.Text.Formatting;
using System.Text.RegularExpressions;

namespace Frostbyte.Extensions
{
    public static class StringExtensions
    {
        public static string LogFormatter(this string str, Exception exception)
        {
            using var sf = new StringFormatter();
            sf.Append($"{str}\n");
            if (exception != null)
                sf.Append($"      Message   : {exception.Message}\n" +
                          $"      Target    : {exception.TargetSite?.Name ?? "Unknown site."}\n" +
                          $"      Source    : {exception.Source ?? "Unknown source."}\n" +
                          $"      Trace     : {exception.StackTrace ?? "Unavailable."}");
            return $"{sf}";
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