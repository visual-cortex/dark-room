using DarkRoom.Core.Enums;
using DarkRoom.Core.Film;
using DarkRoom.Core.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRoom.Core
{
    public class Darkroom
    {
        Negative _original,
                 _internal;

        private Bitmap _image
        {
            get
            {
                return (Bitmap)_internal._image;
            }
            set
            {
                _internal._image = value;
            }
        }

        private static Bitmap _ProcessPixels(Bitmap source, Func<RgbPixel, RgbPixel>  filterLogic)
        {
            const int pixelSize = 4; // 32 bits per pixel
            Bitmap target = new Bitmap(
            source.Width,
            source.Height,
            PixelFormat.Format32bppArgb);

            BitmapData sourceData = null, targetData = null;
            unsafe
            {
                try
                {
                    sourceData = source.LockBits(
                      new Rectangle(0, 0, source.Width, source.Height),
                      ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                    targetData = target.LockBits(
                      new Rectangle(0, 0, target.Width, target.Height),
                      ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                    for (int y = 0; y < source.Height; ++y)
                    {
                        byte* sourceRow = (byte*)sourceData.Scan0 + (y * sourceData.Stride);
                        byte* targetRow = (byte*)targetData.Scan0 + (y * targetData.Stride);

                        for (int x = 0; x < source.Width; ++x)
                        {
                            var alteredPixel = filterLogic(new RgbPixel()
                            {
                                R = sourceRow[x * pixelSize + 2],
                                G = sourceRow[x * pixelSize + 1],
                                B = sourceRow[x * pixelSize + 0],
                                A = sourceRow[x * pixelSize + 3]
                            });

                            targetRow[x * pixelSize + 0] = alteredPixel.B;
                            targetRow[x * pixelSize + 1] = alteredPixel.G;
                            targetRow[x * pixelSize + 2] = alteredPixel.R;
                            targetRow[x * pixelSize + 3] = alteredPixel.A;
                        }
                    }
                }
                finally
                {
                    if (sourceData != null)
                        source.UnlockBits(sourceData);

                    if (targetData != null)
                        target.UnlockBits(targetData);
                }
            }

            return target;
        }

        private static byte Clamp(double pValue)
        {
            return pValue > 255 ? (byte)255 : pValue < 0 ? (byte)0 : (byte)pValue;
        }

        public Darkroom(Negative image)
        {
            _original = image;
            Reset();
        }

        public Darkroom BlackAndWhite(BlackAndWhiteMode mode = BlackAndWhiteMode.Regular)
        {
            double[] ratio = new double[3];
            switch(mode)
            {
                case BlackAndWhiteMode.Regular: ratio = new double[] { 0.3, 0.59, 0.11 }; break;
                case BlackAndWhiteMode.Red: ratio = new double[] { 1, 0, 0 }; break;
                case BlackAndWhiteMode.Green: ratio = new double[] { 0, 1, 0 }; break;
                case BlackAndWhiteMode.Blue: ratio = new double[] { 0, 0, 1 }; break;
            }
            _image = _ProcessPixels(_image, (pixel) => {
                byte result = (byte)(pixel.R * ratio[0] + pixel.G * ratio[1] + pixel.B * ratio[2]);
                pixel.R = result;
                pixel.G = result;
                pixel.B = result;
                return pixel;
            });

            return this;
        }

        public Darkroom Invert()
        {
            _image = _ProcessPixels(_image, (pixel) => {
                pixel.R = (byte)(255 ^ pixel.R);
                pixel.G = (byte)(255 ^ pixel.G);
                pixel.B = (byte)(255 ^ pixel.B);

                return pixel;
            });
            return this;
        }

        public Darkroom Contrast(double value)
        {
            value = value < -100 ? -100 : value > 100 ? 100 : value;

            if (value < 0)
            {
                value *= -1;
                value /= 100 * value;
            }
            value = Math.Pow((value + 100) / 100, 2);

            byte[] contrastLookup = new byte[256];
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
                contrastLookup[i] = Clamp(pValue);
            }

            _image = _ProcessPixels(_image, (pixel) => {
                pixel.R = contrastLookup[pixel.R];
                pixel.G = contrastLookup[pixel.B];
                pixel.B = contrastLookup[pixel.B];

                return pixel;
            });

            return this;
        }

        public Darkroom Brightness(double value)
        {
            value = value < -100 ? -100 : value > 100 ? 100 : value;

            value = Math.Floor(255 * (value / 100));

            double tempPixel;
            _image = _ProcessPixels(_image, (pixel) => {
                tempPixel = pixel.R + value;
                pixel.R = Clamp(tempPixel);
                tempPixel = pixel.G + value;
                pixel.G = Clamp(tempPixel);
                tempPixel = pixel.B + value;
                pixel.B = Clamp(tempPixel);

                return pixel;
            });
            return this;
        }

        public Darkroom Saturation(double value)
        {
            value = value < -100 ? -100 : value > 100 ? 100 : value;

            value *= -0.01;

            // generate saturation lookup for faster processing
            byte[] saturationLookup = new byte[256];

            for (int i = 0; i < 256; i++)
            {
                saturationLookup[i] = Clamp(i * value);
            }

            _image = _ProcessPixels(_image, (pixel) => {
                var max = Math.Max(Math.Max(pixel.R, pixel.G), pixel.B);

                pixel.R += saturationLookup[max - pixel.R];
                pixel.G += saturationLookup[max - pixel.G];
                pixel.B += saturationLookup[max - pixel.B];

                return pixel;
            });

            return this;
        }

        public Darkroom Vibrance(double value)
        {
            value = value < -150 ? -150 : value > 150 ? 150 : value;
            value *= -1;

            _image = _ProcessPixels(_image, (pixel) => {  
                var max = Math.Max(Math.Max(pixel.R, pixel.G), pixel.B);
                var avg = (double)(pixel.R + pixel.G + pixel.B) / 3;
                var amt = ((Math.Abs(max - avg) * 2 / 255) * value) / 100;
                pixel.R = Clamp(pixel.R + (max - pixel.R) * amt);
                pixel.G = Clamp(pixel.G + (max - pixel.G) * amt);
                pixel.B = Clamp(pixel.B + (max - pixel.B) * amt);
                return pixel;
            });
            return this;
        }

        public Darkroom Gammma(double value)
        {
            value = value < -100 ? -100 : value > 100 ? 100 : value;

            if (value >= 0)
                value = 1 - (value / 100);
            else value /= -1;

            byte[] gammaLookup = new byte[256];
            for (int i = 0; i < 256; i++)
                gammaLookup[i] = Clamp(Math.Pow((double)i / 255, value) * 255);

            _image = _ProcessPixels(_image, (pixel) => {
                pixel.R = gammaLookup[pixel.R];
                pixel.G = gammaLookup[pixel.G];
                pixel.B = gammaLookup[pixel.B];
                return pixel;
            });

            return this;
        }

        public Darkroom Noise(double value)
        {
            value = value > 100 ? 100 : value < 0 ? 0 : value;

            var rng = new Random(Environment.TickCount);

            _image = _ProcessPixels(_image, (pixel) => {
                var randomValue = rng.NextDouble() * value * 2.55;
                randomValue = (rng.NextDouble() > 0.5 ? -randomValue : randomValue);
                pixel.R = Clamp(pixel.R + randomValue);
                pixel.B = Clamp(pixel.B + randomValue);
                pixel.G = Clamp(pixel.G + randomValue);
                return pixel;
            });

            return this;
        }

        public Darkroom Sepia(double value = 100)
        {
            value = value > 100 ? 100 : value < 0 ? 0 : value;

            value /= 100;

            _image = _ProcessPixels(_image, (pixel) => {
                pixel.R = Clamp((pixel.R * (1 - (0.607 * value))) + (pixel.G * (0.769 * value)) + (pixel.B * (0.189 * value)));
                pixel.G = Clamp((pixel.R * (0.349 * value)) + (pixel.G * (1 - (0.314 * value))) + (pixel.B * (0.168 * value)));
                pixel.B = Clamp((pixel.R * (0.272 * value)) + (pixel.G * (0.534 * value)) + (pixel.B * (1 - (0.869 * value))));
                return pixel;
            });

            return this;
        }

        public Darkroom Hue(double value)
        {
            value = value > 180 ? 180 : value < -180 ? -180 : value;

            value *= 0.5;
            _image = _ProcessPixels(_image, (pixel) => {
              var hsv = pixel.ToHsv();
              hsv.H *= 100;
              hsv.H += Math.Abs(value);
              hsv.H %= 100;
              hsv.H /= 100;
              return hsv.ToRgb();
            });
            return this;
        }

        public Negative Wash()
        {
            return _internal;
        }

        public void Reset()
        {
            _internal = _original.Clone();
        }
    }
}
