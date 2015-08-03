using DarkRoom.Core.Film;
using DarkRoom.Core.Film.Colorspace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRoom.Core.Utils
{
    internal static class PixelHelper
    {
        internal static PixelHsv ToHsv(this PixelRgb pixel)
        {
            double d, max, min,
                   R, G, B;

            PixelHsv hsvPixel = new PixelHsv();

            R = (double)pixel.R / 255;
            G = (double)pixel.G / 255;
            B = (double)pixel.B / 255;
            max = Math.Max(Math.Max(R, G), B);
            min = Math.Min(Math.Min(R, G), B);
            d = max - min;

            hsvPixel.V = max;
            
            hsvPixel.S = max == 0 ? 0 : d / max;

            if (max == min) {
                hsvPixel.H = 0;
            } 
            else {
                if(max == R)
                   hsvPixel.H = (G - B) / d + (G < B ? 6 : 0);
                if(max == G)
                    hsvPixel.H = (B - R) / d + 2;
                if(max == B)
                    hsvPixel.H = (R - G) / d + 4;
                hsvPixel.H /= 6;
            }

            return hsvPixel;
        }

        internal static PixelRgb ToRgb(this PixelHsv pixel)
        {
            double b = 0, f, g = 0, p, q, r = 0, t;
            int i;
            i = (int)Math.Floor(pixel.H * 6);
            f = pixel.H * 6 - i;
            p = pixel.V * (1 - pixel.S);
            q = pixel.V * (1 - f * pixel.S);
            t = pixel.V * (1 - (1 - f) * pixel.S);

            switch (i % 6)
            {
                case 0:
                    r = pixel.V;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = pixel.V;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = pixel.V;
                    b = t;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = pixel.V;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = pixel.V;
                    break;
                case 5:
                    r = pixel.V;
                    g = p;
                    b = q;
                    break;
            }

            return new PixelRgb()
            {
                R = (byte)Math.Floor(r * 255),
                G = (byte)Math.Floor(g * 255),
                B = (byte)Math.Floor(b * 255),
                A = 255
            };
        }
    }
}
