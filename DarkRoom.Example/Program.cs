using DarkRoom.Core.Film;
using DarkRoom.Core;
using System;
using DarkRoom.Core.Utils;
using DarkRoom.Core.Enums;

namespace DarkRoom.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            Negative img = new Negative("sample.jpg");

            Darkroom editor = new Darkroom(img);
            editor
                    //.BlackAndWhite(BlackAndWhiteMode.Regular)
                    //.Invert()
                    //.Contrast(50)
                    //.Brightness(10)
                    //.Saturation(50)
                    //.Vibrance(-50)
                    //.Gammma(-50)
                    //.Noise(25)
                    //.Sepia()
                    //.Hue(45)
                    .Wash()
                    .Develop(string.Format(@"{0}.png", Environment.TickCount));
        }
    }
}
