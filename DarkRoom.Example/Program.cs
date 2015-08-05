using DarkRoom.Core.Film;
using DarkRoom.Core;
using System;
using DarkRoom.Core.Utils;
using DarkRoom.Core.Enums;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DarkRoom.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            Negative img = new Negative("sample.jpg");
            //MainAsync(img);
            MainSync(img);
            Console.ReadLine();
        }

        static void MainSync(Negative img)
        {
            Stopwatch timer = Stopwatch.StartNew();

            Darkroom editor = new Darkroom(img);

            editor
                .BlackAndWhite(BlackAndWhiteMode.Regular)
                //.Invert()
                //.Contrast(50)
                //.Brightness(10)
                //.Saturation(-50)
                //.Vibrance(-50)
                //.Gammma(-50)
                //.Noise(25)
                //.Sepia()
                //.Hue(45)
                .Wash()
                .Develop(string.Format(@"{0}.jpg", Environment.TickCount));
            timer.Stop();
            Console.WriteLine("Sync method: {0} seconds", timer.Elapsed.TotalSeconds);
        }

        static async void MainAsync(Negative img)
        {
            Stopwatch timer = Stopwatch.StartNew();

            Darkroom editor = new Darkroom(img);

            var edited = await editor
                            .BlackAndWhite(BlackAndWhiteMode.Regular)
                            .Invert()
                            .Contrast(50)
                            .Brightness(10)
                            .Saturation(-50)
                            .Vibrance(-50)
                            .Gammma(-50)
                            .Noise(25)
                            .Sepia()
                            .Hue(45)
                            .WashAsync();

            edited.Develop(string.Format(@"{0}.jpg", Environment.TickCount));

            timer.Stop();

            Console.WriteLine("Async method: {0} seconds", timer.Elapsed.TotalSeconds);
        }
    }
}
