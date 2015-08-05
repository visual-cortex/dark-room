using System;

namespace DarkRoom.Core.Utils.PixelManipulation
{
    internal class FilterValue
    {
        internal static Random RNG;

        internal static readonly double[] BlackAndWhiteRatio;

        static FilterValue()
        {
            RNG = new Random(Environment.TickCount);
            BlackAndWhiteRatio = new double[] { 0.3, 0.59, 0.11 };
        }

        internal static double NormalizeHue(double value)
        {
            value = value > 180 ? 180 : value < -180 ? -180 : value;

            value *= 0.5;

            return value;
        }

        internal static double NormalizeSepia(double value)
        {
            value = value > 100 ? 100 : value < 0 ? 0 : value;

            value /= 100;

            return value;
        }

        internal static double NormalizeVibrance(double value)
        {
            value = value < -150 ? -150 : value > 150 ? 150 : value;
            value *= -1;

            return value;
        }

        internal static double NormalizeNoise(double value)
        {
            value = value > 100 ? 100 : value < 0 ? 0 : value;

            return value;
        }

        internal static byte[] NormalizeGamma(double value)
        {
            value = value < -100 ? -100 : value > 100 ? 100 : value;

            if (value >= 0)
                value = 1 - (value / 100);
            else value /= -1;

            var GammaLookup = new byte[256];

            for (int i = 0; i < 256; i++)
                GammaLookup[i] = PixelHelper.Clamp(Math.Pow((double)i / 255, value) * 255);

            return GammaLookup;
        }

        internal static double NormalizeBrightness(double value)
        {
            value = value < -100 ? -100 : value > 100 ? 100 : value;
            value = Math.Floor(255 * (value / 100));

            return value;
        }

        internal static double[] NormalizeSaturation(double value)
        {
            value = value < -100 ? -100 : value > 100 ? 100 : value;
            value *= -0.01;

            var SaturationLookup = new double[256];

            for (int i = 0; i < 256; i++)
            {
                SaturationLookup[i] = i * value;
            }

            return SaturationLookup;
        }

        internal static byte[] NormalizeContrast(double value)
        {
            value = value < -100 ? -100 : value > 100 ? 100 : value;

            if (value < 0)
            {
                value *= -1;
                value /= 100 * value;
            }
            value = Math.Pow((value + 100) / 100, 2);

            var ContrastLookup = new byte[256];

            for (int i = 0; i < 256; i++)
            {
                double pValue = i;
                pValue = i;
                pValue /= 255;
                pValue -= 0.5;
                pValue *= value;
                pValue += 0.5;
                pValue *= 255;
                ContrastLookup[i] = PixelHelper.Clamp(pValue);
            }

            return ContrastLookup;
        }
    }
}
