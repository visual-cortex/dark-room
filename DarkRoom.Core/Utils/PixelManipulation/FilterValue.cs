using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRoom.Core.Utils.PixelManipulation
{
    internal static class FilterValue
    {
        internal static double[] BlackAndWhiteRatio;
        internal static byte[] ContrastLookup = new byte[256],
                               SaturationLookup = new byte[256];

        internal static void SetSaturationLookup(double value)
        {
            byte[] saturationLookup = new byte[256];

            for (int i = 0; i < 256; i++)
            {
                saturationLookup[i] = Clamp(i * value);
            }
        }

        internal static double NormalizeBrightness(double value)
        {
            value = value < -100 ? -100 : value > 100 ? 100 : value;
            value = Math.Floor(255 * (value / 100));

            return value;
        }

        internal static double NormalizeSaturation(double value)
        {
            value = value < -100 ? -100 : value > 100 ? 100 : value;
            value *= -0.01;

            return value;
        }

        private static byte Clamp(double pValue)
        {
            return pValue > 255 ? (byte)255 : pValue < 0 ? (byte)0 : (byte)pValue;
        }
    }
}
