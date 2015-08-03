using DarkRoom.Core.Enums;
using DarkRoom.Core.Film.Colorspace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRoom.Core.Utils.PixelManipulation
{
    internal static class FilterLogic
    {
        static FilterLogic()
        {
            FilterValue.BlackAndWhiteRatio = new double[] { 0.3, 0.59, 0.11 };
        }

        internal static PixelRgb Invert(this PixelRgb pixel)
        {
            pixel.R = (byte)(255 ^ pixel.R);
            pixel.G = (byte)(255 ^ pixel.G);
            pixel.B = (byte)(255 ^ pixel.B);

            return pixel;
        }

        internal static PixelRgb BlackAndWhite(this PixelRgb pixel, BlackAndWhiteMode mode = BlackAndWhiteMode.Regular)
        {
            byte result = 0;

            switch(mode)
            {
                case BlackAndWhiteMode.Regular: result = (byte)((double)pixel.R * FilterValue.BlackAndWhiteRatio[0] + (double)pixel.G * FilterValue.BlackAndWhiteRatio[1] + (double)pixel.B * FilterValue.BlackAndWhiteRatio[2]); break;
                case BlackAndWhiteMode.Red: result = pixel.R; break;
                case BlackAndWhiteMode.Green: result = pixel.G; break;
                case BlackAndWhiteMode.Blue: result = pixel.B; break;
            }

            pixel.R = result;
            pixel.G = result;
            pixel.B = result;

            return pixel;
        }

        internal static void SetContrast(double value)
        {
            value = value < -100 ? -100 : value > 100 ? 100 : value;

            if (value < 0)
            {
                value *= -1;
                value /= 100 * value;
            }
            value = Math.Pow((value + 100) / 100, 2);

            // create contrast value lookup for faster proccesing
            for (int i = 0; i < 256; i++)
            {
                double pValue = i;
                pValue = i;
                pValue /= 255;
                pValue -= 0.5;
                pValue *= value;
                pValue += 0.5;
                pValue *= 255;
                FilterValue.ContrastLookup[i] = Clamp(pValue);
            }
        }

        internal static PixelRgb Contrast(this PixelRgb pixel)
        {
            pixel.R = FilterValue.ContrastLookup[pixel.R];
            pixel.G = FilterValue.ContrastLookup[pixel.G];
            pixel.B = FilterValue.ContrastLookup[pixel.B];
            return pixel;
        }

        internal static PixelRgb Brightness(this PixelRgb pixel, double value)
        {
            pixel.R = Clamp(pixel.R + value);
            pixel.G = Clamp(pixel.G + value);
            pixel.B = Clamp(pixel.B + value);

            return pixel;
        }

        private static byte Clamp(double pValue)
        {
            return pValue > 255 ? (byte)255 : pValue < 0 ? (byte)0 : (byte)pValue;
        }
    }
}
