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
            Stopwatch cropTimer = Stopwatch.StartNew();

            Negative img = new Negative("sample.jpg", 1280, 720)
                                       .Cut(560, 0, 720, 720);
            cropTimer.Stop();
            Console.WriteLine("Resized and cropped: {0} seconds", cropTimer.Elapsed.TotalSeconds);

            Stopwatch processingTimer = Stopwatch.StartNew();

            using (Darkroom editor = new Darkroom(img))
            {
                editor
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
                    .Tint("#efefef")
                    .Wash()
                    .Develop(string.Format(@"{0}.jpg", Environment.TickCount));
            }

            processingTimer.Stop();

            Console.WriteLine("All image filters applied: {0} seconds", processingTimer.Elapsed.TotalSeconds);
            Console.ReadLine();
        }
    }
}
