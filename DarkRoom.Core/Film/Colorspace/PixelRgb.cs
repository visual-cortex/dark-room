using DarkRoom.Core.Enums;
using DarkRoom.Core.Utils.PixelManipulation;
using System;

namespace DarkRoom.Core.Film.Colorspace
{
    internal class PixelRgb
    {
        internal byte R { get; set; }
        internal byte G { get; set; }
        internal byte B { get; set; }
        internal byte A { get; set; }

        internal PixelRgb Invert()
        {
            this.R = (byte)(255 ^ this.R);
            this.G = (byte)(255 ^ this.G);
            this.B = (byte)(255 ^ this.B);

            return this;
        }

        internal PixelRgb BlackAndWhite(BlackAndWhiteMode mode = BlackAndWhiteMode.Regular)
        {
            byte result = 0;

            switch (mode)
            {
                case BlackAndWhiteMode.Regular:
                    result = (byte)(this.R * FilterValue.BlackAndWhiteRatio[0] +
                                    this.G * FilterValue.BlackAndWhiteRatio[1] +
                                    this.B * FilterValue.BlackAndWhiteRatio[2]);
                    break;

                case BlackAndWhiteMode.Red:
                    result = this.R;
                    break;

                case BlackAndWhiteMode.Green:
                    result = this.G;
                    break;

                case BlackAndWhiteMode.Blue:
                    result = this.B;
                    break;
            }

            this.R = result;
            this.G = result;
            this.B = result;

            return this;
        }

        internal PixelRgb Contrast( byte[] lookup)
        {
            this.R = lookup[this.R];
            this.G = lookup[this.G];
            this.B = lookup[this.B];
            return this;
        }

        internal PixelRgb Brightness( double value)
        {
            this.R = PixelHelper.Clamp(this.R + value);
            this.G = PixelHelper.Clamp(this.G + value);
            this.B = PixelHelper.Clamp(this.B + value);

            return this;
        }

        internal PixelRgb Saturation( double[] lookup)
        {
            var max = Math.Max(Math.Max(this.R, this.G), this.B);

            this.R = PixelHelper.Clamp(this.R + lookup[max - this.R]);
            this.G = PixelHelper.Clamp(this.G + lookup[max - this.G]);
            this.B = PixelHelper.Clamp(this.B + lookup[max - this.B]);

            return this;
        }

        internal PixelRgb Vibrance( double value)
        {
            var max = Math.Max(Math.Max(this.R, this.G), this.B);
            var avg = (double)(this.R + this.G + this.B) / 3;
            var amt = ((Math.Abs(max - avg) * 2 / 255) * value) / 100;

            this.R = PixelHelper.Clamp(this.R + (max - this.R) * amt);
            this.G = PixelHelper.Clamp(this.G + (max - this.G) * amt);
            this.B = PixelHelper.Clamp(this.B + (max - this.B) * amt);

            return this;
        }

        internal PixelRgb Gamma( byte[] lookup)
        {
            this.R = lookup[this.R];
            this.G = lookup[this.G];
            this.B = lookup[this.B];

            return this;
        }

        internal PixelRgb Noise( double value)
        {
            var randomValue = FilterValue.RNG.NextDouble() * value * 2.55;
            randomValue = (FilterValue.RNG.NextDouble() > 0.5 ? -randomValue : randomValue);
            this.R = PixelHelper.Clamp(this.R + randomValue);
            this.B = PixelHelper.Clamp(this.B + randomValue);
            this.G = PixelHelper.Clamp(this.G + randomValue);

            return this;
        }

        internal PixelRgb Sepia( double value)
        {
            this.R = PixelHelper.Clamp((this.R * (1 - (0.607 * value))) + (this.G * (0.769 * value)) + (this.B * (0.189 * value)));
            this.G = PixelHelper.Clamp((this.R * (0.349 * value)) + (this.G * (1 - (0.314 * value))) + (this.B * (0.168 * value)));
            this.B = PixelHelper.Clamp((this.R * (0.272 * value)) + (this.G * (0.534 * value)) + (this.B * (1 - (0.869 * value))));

            return this;
        }

        internal PixelRgb Hue( double value)
        {
            var hsv = this.ToHsv();
            hsv.H *= 100;
            hsv.H += Math.Abs(value);
            hsv.H %= 100;
            hsv.H /= 100;
            return hsv.ToRgb();
        }

        internal PixelRgb Tint(HexColor color)
        {
            this.B = PixelHelper.Clamp((this.B + (255 - this.B) * ((double)color.Pixel.B / 255)));
            this.R = PixelHelper.Clamp((this.R + (255 - this.R) * ((double)color.Pixel.R / 255)));
            this.G = PixelHelper.Clamp((this.G + (255 - this.G) * ((double)color.Pixel.G / 255)));

            return this;
        }
    }
}
