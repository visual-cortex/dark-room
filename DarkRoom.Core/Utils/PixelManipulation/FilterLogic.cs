﻿using DarkRoom.Core.Enums;
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

        internal static PixelRgb Contrast(this PixelRgb pixel)
        {
            pixel.R = FilterValue.ContrastLookup[pixel.R];
            pixel.G = FilterValue.ContrastLookup[pixel.G];
            pixel.B = FilterValue.ContrastLookup[pixel.B];
            return pixel;
        }

        internal static PixelRgb Brightness(this PixelRgb pixel)
        {
            pixel.R = PixelHelper.Clamp(pixel.R + FilterValue.Brightness);
            pixel.G = PixelHelper.Clamp(pixel.G + FilterValue.Brightness);
            pixel.B = PixelHelper.Clamp(pixel.B + FilterValue.Brightness);

            return pixel;
        }

        internal static PixelRgb Saturation(this PixelRgb pixel)
        {
            var max = Math.Max(Math.Max(pixel.R, pixel.G), pixel.B);

            pixel.R = PixelHelper.Clamp(pixel.R + FilterValue.SaturationLookup[max - pixel.R]);
            pixel.G = PixelHelper.Clamp(pixel.G + FilterValue.SaturationLookup[max - pixel.G]);
            pixel.B = PixelHelper.Clamp(pixel.B + FilterValue.SaturationLookup[max - pixel.B]);

            return pixel;
        }

        internal static PixelRgb Vibrance(this PixelRgb pixel)
        {
            var max = Math.Max(Math.Max(pixel.R, pixel.G), pixel.B);
            var avg = (double)(pixel.R + pixel.G + pixel.B) / 3;
            var amt = ((Math.Abs(max - avg) * 2 / 255) * FilterValue.Vibrance) / 100;

            pixel.R = PixelHelper.Clamp(pixel.R + (max - pixel.R) * amt);
            pixel.G = PixelHelper.Clamp(pixel.G + (max - pixel.G) * amt);
            pixel.B = PixelHelper.Clamp(pixel.B + (max - pixel.B) * amt);

            return pixel;
        }

        internal static PixelRgb Gamma(this PixelRgb pixel)
        {
            pixel.R = FilterValue.GammaLookup[pixel.R];
            pixel.G = FilterValue.GammaLookup[pixel.G];
            pixel.B = FilterValue.GammaLookup[pixel.B];

            return pixel;
        }

        internal static PixelRgb Noise(this PixelRgb pixel)
        {
            var randomValue = FilterValue.RNG.NextDouble() * FilterValue.Noise * 2.55;
            randomValue = (FilterValue.RNG.NextDouble() > 0.5 ? -randomValue : randomValue);
            pixel.R = PixelHelper.Clamp(pixel.R + randomValue);
            pixel.B = PixelHelper.Clamp(pixel.B + randomValue);
            pixel.G = PixelHelper.Clamp(pixel.G + randomValue);

            return pixel;
        }

        internal static PixelRgb Sepia(this PixelRgb pixel)
        {
            double value = FilterValue.Sepia;

            pixel.R = PixelHelper.Clamp((pixel.R * (1 - (0.607 * value))) + (pixel.G * (0.769 * value)) + (pixel.B * (0.189 * value)));
            pixel.G = PixelHelper.Clamp((pixel.R * (0.349 * value)) + (pixel.G * (1 - (0.314 * value))) + (pixel.B * (0.168 * value)));
            pixel.B = PixelHelper.Clamp((pixel.R * (0.272 * value)) + (pixel.G * (0.534 * value)) + (pixel.B * (1 - (0.869 * value))));

            return pixel;
        }

        internal static PixelRgb Hue(this PixelRgb pixel)
        {
            var hsv = pixel.ToHsv();
            hsv.H *= 100;
            hsv.H += Math.Abs(FilterValue.Hue);
            hsv.H %= 100;
            hsv.H /= 100;
            return hsv.ToRgb();
        }
    }
}