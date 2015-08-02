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
            //Negative img = new Negative(new Uri("http://i0.wp.com/thezinx.com/wp-content/uploads/wallpaper_1.png"));
            //DataUri data = new DataUri("'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQAQMAAAAlPW0iAAAABlBMVEUAAAD///+l2Z/dAAAAM0lEQVR4nGP4/5/h/1+G/58ZDrAz3D/McH8yw83NDDeNGe4Ug9C9zwz3gVLMDA/A6P9/AFGGFyjOXZtQAAAAAElFTkSuQmCC'");
            //Negative deserialized = img.Digitize().ToNegative();
            //deserialized.Develop("cacat." + data.Mime.ToString().ToLower(), data.Mime);
            //Darkroom editor = new Darkroom(img);
            //Console.WriteLine(img.Digitize().ToString().Substring(0, 50));
            //Console.ReadLine();
            // editor
            //.BlackAndWhite()
            //.Invert()
            //.Contrast(30)
            //.Brightness(50)
            //.Saturation(-100)
            //.Vibrance(100)
            //.Wash()
            //.Develop(string.Format(@"{0}.png", Environment.TickCount), height: 640);
        }
    }
}
