using System;
using System.Drawing;
using Concept.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Console = Colorful.Console;

namespace Concept
{
    public static class Extensions
    {
        public static AuthenticationBuilder UseHeaderAuthentication(this AuthenticationBuilder builder,
            string authenticationScheme, Action<HeaderOptions> configureOptions)
            => builder.AddScheme<HeaderOptions, HeaderAuthenticationHandler>(authenticationScheme, configureOptions);

        public static AuthenticationBuilder UseHeaderAuthentication(this AuthenticationBuilder builder,
            Action<HeaderOptions> configureOptions)
            => builder.AddScheme<HeaderOptions, HeaderAuthenticationHandler>("HeaderAuth", configureOptions);

        public static LogLevel ConvertTo(this Vysn.Commons.LogLevel logLevel)
            => logLevel switch
            {
                Vysn.Commons.LogLevel.Debug       => LogLevel.Debug,
                Vysn.Commons.LogLevel.Exception   => LogLevel.Critical,
                Vysn.Commons.LogLevel.Information => LogLevel.Information,
                Vysn.Commons.LogLevel.Warning     => LogLevel.Warning,
                _                                 => LogLevel.Trace
            };

        public static void Append(string message, Color color)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
        }
    }
}