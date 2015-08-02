using DarkRoom.Core.Film;
using System;

namespace DarkRoom.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            Negative img = new Negative(new Uri("http://i0.wp.com/thezinx.com/wp-content/uploads/wallpaper_1.png"));
            img.Develop(string.Format(@"{0}.png", Environment.TickCount), height: 640);
        }
    }
}
