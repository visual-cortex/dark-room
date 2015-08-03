using DarkRoom.Core.Enums;
using DarkRoom.Core.Film;
using DarkRoom.Core.Film.Colorspace;
using DarkRoom.Core.Utils;
using DarkRoom.Core.Utils.PixelManipulation;
using System;
using System.Drawing;
using System.Drawing.Imaging;

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

        private void _ProcessPixels(Func<PixelRgb, PixelRgb>  filterLogic)
        {
            // 32 bits per pixel
            const int pixelSize = 4; 

            BitmapData sourceData = null;
            unsafe
            {
                try
                {
                    sourceData = _image.LockBits(
                      new Rectangle(0, 0, _image.Width, _image.Height),
                      ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                    for (int y = 0; y < _image.Height; ++y)
                    {
                        byte* sourceRow = (byte*)sourceData.Scan0 + (y * sourceData.Stride);

                        for (int x = 0; x < _image.Width; ++x)
                        {
                            var alteredPixel = filterLogic(new PixelRgb()
                            {
                                R = sourceRow[x * pixelSize + 2],
                                G = sourceRow[x * pixelSize + 1],
                                B = sourceRow[x * pixelSize + 0],
                                A = sourceRow[x * pixelSize + 3]
                            });

                            sourceRow[x * pixelSize + 0] = alteredPixel.B;
                            sourceRow[x * pixelSize + 1] = alteredPixel.G;
                            sourceRow[x * pixelSize + 2] = alteredPixel.R;
                            sourceRow[x * pixelSize + 3] = alteredPixel.A;
                        }
                    }
                }
                finally
                {
                    if (sourceData != null)
                        _image.UnlockBits(sourceData);
                }
            }
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
            _ProcessPixels((pixel) => {
                return pixel.BlackAndWhite(mode);
            });

            return this;
        }

        public Darkroom Invert()
        {
            _ProcessPixels((pixel) => {
                return pixel.Invert();
            });
            return this;
        }

        public Darkroom Contrast(double value)
        {
            FilterLogic.SetContrast(value);

            _ProcessPixels((pixel) => {
                return pixel.Contrast();
            });

            return this;
        }

        public Darkroom Brightness(double value)
        {
            _ProcessPixels((pixel) => {
                return pixel.Brightness(FilterValue.NormalizeBrightness(value));
            });
            return this;
        }

        public Darkroom Saturation(double value)
        {
            FilterValue.SetSaturationLookup(value);

            _ProcessPixels((pixel) => {
                var max = Math.Max(Math.Max(pixel.R, pixel.G), pixel.B);

                pixel.R += FilterValue.SaturationLookup[max - pixel.R];
                pixel.G += FilterValue.SaturationLookup[max - pixel.G];
                pixel.B += FilterValue.SaturationLookup[max - pixel.B];

                return pixel;
            });

            return this;
        }

        public Darkroom Vibrance(double value)
        {
            value = value < -150 ? -150 : value > 150 ? 150 : value;
            value *= -1;

            _ProcessPixels((pixel) => {  
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

            _ProcessPixels((pixel) => {
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

            _ProcessPixels((pixel) => {
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

            _ProcessPixels((pixel) => {
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
            _ProcessPixels((pixel) => {
              var hsv = pixel.ToHsv();
              hsv.H *= 100;
              hsv.H += Math.Abs(value);
              hsv.H %= 100;
              hsv.H /= 100;
              return hsv.ToRgb();
            });
            return this;
        }

        public Darkroom Tint(string hex)
        {
            return Tint(new HexColor(hex));
        }

        public Darkroom Tint(byte red, byte green, byte blue)
        {
            string hex = string.Format("#{0}{1}{2}", red.ToString("X2"), green.ToString("X2"), blue.ToString("X2"));
            return Tint(hex);
        }

        public Darkroom Tint(Color color)
        {
            string hex = string.Format("#{0}{1}{2}", color.R.ToString("X2"), color.G.ToString("X2"), color.B.ToString("X2"));
            return Tint(hex);
        }

        public Darkroom Tint(HexColor color)
        {
            _ProcessPixels((pixel) => {
                pixel.B = Clamp((pixel.B + (255 - pixel.B) * ((double)color.Pixel.B / 255)));
                pixel.R = Clamp((pixel.R + (255 - pixel.R) * ((double)color.Pixel.R / 255)));
                pixel.G = Clamp((pixel.G + (255 - pixel.G) * ((double)color.Pixel.G / 255)));
                return pixel;
            });
            return this;
        }

        public Darkroom Pixelate(int size)
        {
            PixelRgb currentPixel = null;
            int currentPosition = 0;

            _ProcessPixels((pixel) => {
                if (currentPosition == 0 || currentPosition % size == 0)
                {
                    currentPixel = pixel;
                }

                currentPosition++;

                return currentPixel;
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
