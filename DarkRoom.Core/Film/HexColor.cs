using DarkRoom.Core.Film.Colorspace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRoom.Core.Film
{
    public class HexColor
    {
        internal PixelRgb Pixel { get; private set; }

        public string Hex { get; private set; }
        public byte[] Channels { get; private set; }

        public HexColor(string hex)
        {
            Hex = hex;
            hex = hex.Replace("#", "");

            if (hex.Length != 6)
                throw new Exception(string.Format("#{0} is an invalid color code.", hex));

            Channels = Enumerable.Range(0, hex.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                     .ToArray();

            Pixel = new PixelRgb();

            Pixel.R = Channels[0];
            Pixel.G = Channels[1];
            Pixel.B = Channels[2];
        }
    }
}
