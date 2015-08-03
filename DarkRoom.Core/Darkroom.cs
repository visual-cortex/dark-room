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
            FilterValue.Contrast = value;

            _ProcessPixels((pixel) => {
                return pixel.Contrast();
            });

            return this;
        }

        public Darkroom Brightness(double value)
        {
            FilterValue.Brightness = value;

            _ProcessPixels((pixel) => {
                return pixel.Brightness();
            });

            return this;
        }

        public Darkroom Saturation(double value)
        {
            FilterValue.Saturation = value;

            _ProcessPixels((pixel) => {
                return pixel.Saturation();
            });

            return this;
        }

        public Darkroom Vibrance(double value)
        {
            FilterValue.Vibrance = value;

            _ProcessPixels((pixel) => {  
                return pixel.Vibrance();
            });

            return this;
        }

        public Darkroom Gammma(double value)
        {
            FilterValue.Gamma = value;

            _ProcessPixels((pixel) => {
                return pixel.Gamma();
            });

            return this;
        }

        public Darkroom Noise(double value)
        {
            FilterValue.Noise = value;

            _ProcessPixels((pixel) => {
                return pixel.Noise();
            });

            return this;
        }

        public Darkroom Sepia(double value = 100)
        {
            FilterValue.Sepia = value;

            _ProcessPixels((pixel) => {
                return pixel.Sepia();
            });

            return this;
        }

        public Darkroom Hue(double value)
        {
            FilterValue.Hue = value;

            _ProcessPixels((pixel) => {
                return pixel.Hue();
            });

            return this;
        }

        /* TO-DO 
         * 
         * Separate processing logic.
         */
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
                pixel.B = PixelHelper.Clamp((pixel.B + (255 - pixel.B) * ((double)color.Pixel.B / 255)));
                pixel.R = PixelHelper.Clamp((pixel.R + (255 - pixel.R) * ((double)color.Pixel.R / 255)));
                pixel.G = PixelHelper.Clamp((pixel.G + (255 - pixel.G) * ((double)color.Pixel.G / 255)));
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
