using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Colorful;
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


        public static (Color Color, string Abbrevation) LogLevelInfo(this LogLevel logLevel)
            => logLevel switch
            {
                LogLevel.Information => (Color.SpringGreen, "INFO"),
                LogLevel.Debug       => (Color.MediumPurple, "DBUG"),
                LogLevel.Trace       => (Color.MediumPurple, "TRCE"),
                LogLevel.Critical    => (Color.Crimson, "CRIT"),
                LogLevel.Error       => (Color.Crimson, "EROR"),
                LogLevel.Warning     => (Color.Orange, "WARN"),
                _                    => (Color.Tomato, "UKNW")
            };

        public static void PrintHeaderAndInformation()
        {
            const string logo =
                @"
             ▄▄·        ▐ ▄  ▄▄· ▄▄▄ . ▄▄▄·▄▄▄▄▄
            ▐█ ▌▪▪     •█▌▐█▐█ ▌▪▀▄.▀·▐█ ▄█•██  
            ██ ▄▄ ▄█▀▄ ▐█▐▐▌██ ▄▄▐▀▀▪▄ ██▀· ▐█.▪
            ▐███▌▐█▌.▐▌██▐█▌▐███▌▐█▄▄▌▐█▪·• ▐█▌·
            ·▀▀▀  ▀█▄▀▪▀▀ █▪·▀▀▀  ▀▀▀ .▀    ▀▀▀ 
";
            
            var lineBreak = new string('-', 105);
            Console.WriteLine(logo, Color.Crimson);
            Console.WriteLine(lineBreak);
            
            const string logMessage = "    Framework: {0} - OS Arch: {1} - Process Arch: {2} - OS: {3}";
            var formatters = new[]
            {
                new Formatter(RuntimeInformation.FrameworkDescription, Color.Aqua),
                new Formatter(RuntimeInformation.OSArchitecture, Color.Gold),
                new Formatter(RuntimeInformation.ProcessArchitecture, Color.LawnGreen),
                new Formatter(RuntimeInformation.OSDescription, Color.HotPink)
            };

            Console.WriteLineFormatted(logMessage, Color.White, formatters);
            Console.WriteLine(lineBreak);
        }
    }
}