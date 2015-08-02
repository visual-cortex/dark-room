using DarkRoom.Core.Film;
using DarkRoom.Core;
using System;
using DarkRoom.Core.Utils;

namespace DarkRoom.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            Negative img = new Negative(new Uri("http://i0.wp.com/thezinx.com/wp-content/uploads/wallpaper_1.png")),
                     img2 = img.Clone();
           
            //if(img != img2)
            //{
            //    Console.WriteLine("Different images");
            //    return;
            //}
            //else
            //{
            //    Console.WriteLine("Exact images");
            //    Console.ReadLine();
            //}

            Darkroom editor = new Darkroom(img);
            editor
                //.BlackAndWhite()
                //.Invert()
                //.Contrast(30)
                //.Brightness(50)
                //.Saturation(-100)
                //.Vibrance(100)
                .Wash()
                .Develop(string.Format(@"{0}.png", Environment.TickCount), height: 640);
        }
    }
}
