using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRoom.Core.Utils.PixelManipulation
{
    internal static class FilterValue
    {
        internal static Random RNG = new Random(Environment.TickCount);

        internal static readonly double[] BlackAndWhiteRatio = new double[] { 0.3, 0.59, 0.11 };

        internal static byte[] ContrastLookup = new byte[256],
                               GammaLookup = new byte[256];

        internal static double[] SaturationLookup = new double[256];

        private static double _brightness;
        internal static double Brightness
        {
            get
            {
                return _brightness;
            }
            set
            {
                _brightness = NormalizeBrightness(value);
            }
        }

        private static double _vibrance;
        internal static double Vibrance
        {
            get
            {
                return _vibrance;
            }
            set
            {
                _vibrance = NormalizeVibrance(value);
            }
        }

        private static double _noise;
        internal static double Noise
        {
            get
            {
                return _noise;
            }
            set
            {
                _noise = NormalizeNoise(value);
            }
        }

        private static double _sepia;
        internal static double Sepia
        {
            get
            {
                return _sepia;
            }
            set
            {
                _sepia = NormalizeSepia(value);
            }
        }

        private static double _hue;
        internal static double Hue
        {
            get
            {
                return _hue;
            }
            set
            {
                _hue = NormalizeHue(value);
            }
        }

        internal static double Contrast
        {
            set
            {
                var normalized = NormalizeContrast(value);
                // create contrast value lookup for faster proccesing
                for (int i = 0; i < 256; i++)
                {
                    double pValue = i;
                    pValue = i;
                    pValue /= 255;
                    pValue -= 0.5;
                    pValue *= normalized;
                    pValue += 0.5;
                    pValue *= 255;
                    FilterValue.ContrastLookup[i] = PixelHelper.Clamp(pValue);
                }
            }
        }

        internal static double Saturation
        {
            set
            {
                var normalized = NormalizeSaturation(value);

                for (int i = 0; i < 256; i++)
                {
                    SaturationLookup[i] = i * normalized;
                }
            }
        }

        internal static double Gamma
        {
            set
            {
                var normalized = NormalizeGamma(value);
                for (int i = 0; i < 256; i++)
                    GammaLookup[i] = PixelHelper.Clamp(Math.Pow((double)i / 255, normalized) * 255);
            }
        }

        private static double NormalizeHue(double value)
        {
            value = value > 180 ? 180 : value < -180 ? -180 : value;

            value *= 0.5;

            return value;
        }

        private static double NormalizeSepia(double value)
        {
            value = value > 100 ? 100 : value < 0 ? 0 : value;

            value /= 100;

            return value;
        }

        private static double NormalizeVibrance(double value)
        {
            value = value < -150 ? -150 : value > 150 ? 150 : value;
            value *= -1;

            return value;
        }

        private static double NormalizeNoise(double value)
        {
            value = value > 100 ? 100 : value < 0 ? 0 : value;

            return value;
        }

        private static double NormalizeGamma(double value)
        {
            value = value < -100 ? -100 : value > 100 ? 100 : value;

            if (value >= 0)
                value = 1 - (value / 100);
            else value /= -1;

            return value;
        }
        
        private static double NormalizeBrightness(double value)
        {
            value = value < -100 ? -100 : value > 100 ? 100 : value;
            value = Math.Floor(255 * (value / 100));

            return value;
        }

        private static double NormalizeSaturation(double value)
        {
            value = value < -100 ? -100 : value > 100 ? 100 : value;
            value *= -0.01;

            return value;
        }

        private static double NormalizeContrast(double value)
        {
            value = value < -100 ? -100 : value > 100 ? 100 : value;

            if (value < 0)
            {
                value *= -1;
                value /= 100 * value;
            }
            value = Math.Pow((value + 100) / 100, 2);

            return value;
        }
    }
}
