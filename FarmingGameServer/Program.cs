using System;
using System.Threading;
using CommandLine;
namespace FarmingGameServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ComanndLineArgs>(args).WithParsed(Run);
        }

        static void Run(ComanndLineArgs args)
        {
            Console.WriteLine("server running");
            App app = new App();
            app.URL = args.url;
            app.Init();

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Windows.Timer timer = new Windows.Timer();
                timer.Interval = TimeSpan.FromMilliseconds(10);
                timer.Elapsed += (s, e) =>
                {
                    app.Tick();
                };
                Console.WriteLine("win32");
                timer.Start();
                while (true)
                {
                    Console.ReadLine();
                }
            }
            else
            {
                while (true)
                {
                    app.Tick();
                    Thread.Sleep(Math.Max((int)(app.deltaTime * 1000), 1));
                }
            }
        }
    }
}
