using System;
using System.Text.Formatting;

namespace Frostbyte.Extensions
{
    public static class StringExtensions
    {
        public static string LogFormatter(this string str, Exception exception)
        {
            using var sf = new StringFormatter();
            sf.Append($"{str}\n");
            if (exception != null)
                sf.Append(
                    $"      Message   : {exception.Message}\n" +
                    $"      Target    : {exception.TargetSite.Name ?? "Unknown site."}\n" +
                    $"      Source    : {exception.Source ?? "Unknown source."}\n" +
                    $"      Trace     : {exception.StackTrace ?? "Unavailable."}");
            return $"{sf}";
        }
    }
}