using DarkRoom.Core.Enums;
using DarkRoom.Core.Film;
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

        private static List<FilterValue> imageValues = new List<FilterValue>();
        
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

            switch (mode)
            {
                case BlackAndWhiteMode.Regular:
                    result = (byte)(pixel.R * FilterValue.BlackAndWhiteRatio[0] +
                                    pixel.G * FilterValue.BlackAndWhiteRatio[1] +
                                    pixel.B * FilterValue.BlackAndWhiteRatio[2]);
                    break;

                case BlackAndWhiteMode.Red:
                    result = pixel.R;
                    break;

                case BlackAndWhiteMode.Green:
                    result = pixel.G;
                    break;

                case BlackAndWhiteMode.Blue:
                    result = pixel.B;
                    break;
            }

            pixel.R = result;
            pixel.G = result;
            pixel.B = result;

            return pixel;
        }

        internal static PixelRgb Contrast(this PixelRgb pixel, byte[] lookup)
        {
            pixel.R = lookup[pixel.R];
            pixel.G = lookup[pixel.G];
            pixel.B = lookup[pixel.B];
            return pixel;
        }

        internal static PixelRgb Brightness(this PixelRgb pixel, double value)
        {
            pixel.R = PixelHelper.Clamp(pixel.R + value);
            pixel.G = PixelHelper.Clamp(pixel.G + value);
            pixel.B = PixelHelper.Clamp(pixel.B + value);

            return pixel;
        }

        internal static PixelRgb Saturation(this PixelRgb pixel, double[] lookup)
        {
            var max = Math.Max(Math.Max(pixel.R, pixel.G), pixel.B);

            pixel.R = PixelHelper.Clamp(pixel.R + lookup[max - pixel.R]);
            pixel.G = PixelHelper.Clamp(pixel.G + lookup[max - pixel.G]);
            pixel.B = PixelHelper.Clamp(pixel.B + lookup[max - pixel.B]);

            return pixel;
        }

        internal static PixelRgb Vibrance(this PixelRgb pixel, double value)
        {
            var max = Math.Max(Math.Max(pixel.R, pixel.G), pixel.B);
            var avg = (double)(pixel.R + pixel.G + pixel.B) / 3;
            var amt = ((Math.Abs(max - avg) * 2 / 255) * value) / 100;

            pixel.R = PixelHelper.Clamp(pixel.R + (max - pixel.R) * amt);
            pixel.G = PixelHelper.Clamp(pixel.G + (max - pixel.G) * amt);
            pixel.B = PixelHelper.Clamp(pixel.B + (max - pixel.B) * amt);

            return pixel;
        }

        internal static PixelRgb Gamma(this PixelRgb pixel, byte[] lookup)
        {
            pixel.R = lookup[pixel.R];
            pixel.G = lookup[pixel.G];
            pixel.B = lookup[pixel.B];

            return pixel;
        }

        internal static PixelRgb Noise(this PixelRgb pixel, double value)
        {
            var randomValue = FilterValue.RNG.NextDouble() * value * 2.55;
            randomValue = (FilterValue.RNG.NextDouble() > 0.5 ? -randomValue : randomValue);
            pixel.R = PixelHelper.Clamp(pixel.R + randomValue);
            pixel.B = PixelHelper.Clamp(pixel.B + randomValue);
            pixel.G = PixelHelper.Clamp(pixel.G + randomValue);

            return pixel;
        }

        internal static PixelRgb Sepia(this PixelRgb pixel, double value)
        {
            pixel.R = PixelHelper.Clamp((pixel.R * (1 - (0.607 * value))) + (pixel.G * (0.769 * value)) + (pixel.B * (0.189 * value)));
            pixel.G = PixelHelper.Clamp((pixel.R * (0.349 * value)) + (pixel.G * (1 - (0.314 * value))) + (pixel.B * (0.168 * value)));
            pixel.B = PixelHelper.Clamp((pixel.R * (0.272 * value)) + (pixel.G * (0.534 * value)) + (pixel.B * (1 - (0.869 * value))));

            return pixel;
        }

        internal static PixelRgb Hue(this PixelRgb pixel, double value)
        {
            var hsv = pixel.ToHsv();
            hsv.H *= 100;
            hsv.H += Math.Abs(value);
            hsv.H %= 100;
            hsv.H /= 100;
            return hsv.ToRgb();
        }

        internal static PixelRgb Tint(this PixelRgb pixel, HexColor color)
        {
            pixel.B = PixelHelper.Clamp((pixel.B + (255 - pixel.B) * ((double)color.Pixel.B / 255)));
            pixel.R = PixelHelper.Clamp((pixel.R + (255 - pixel.R) * ((double)color.Pixel.R / 255)));
            pixel.G = PixelHelper.Clamp((pixel.G + (255 - pixel.G) * ((double)color.Pixel.G / 255)));

            return pixel;
        }
    }
}
