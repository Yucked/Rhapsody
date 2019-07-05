using System;
using System.Threading.Tasks;
using Frostbyte.Handlers;
using Console = Colorful.Console;

namespace Frostbyte
{
    public sealed class Program
    {
        public static Task Main()
        {
            Console.Title = "Frostbyte - Yucked";
            Console.WindowHeight = 25;
            Console.WindowWidth = 140;

            AppDomain.CurrentDomain.UnhandledException += (s, e)
                => LogHandler<Program>.Log.Error(exception: e.ExceptionObject as Exception);

            return new MainHandler().InitializeAsync();
        }
    }
}