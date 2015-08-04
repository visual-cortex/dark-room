using DarkRoom.Core.Enums;
using DarkRoom.Core.Film;
using DarkRoom.Core.Film.Colorspace;
using DarkRoom.Core.Utils;
using DarkRoom.Core.Utils.PixelManipulation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace DarkRoom.Core
{
    public class Darkroom
    {
        private Negative _original,
                         _internal;

        private List<Filter> appliedFilters = new List<Filter>();

        private Guid Uuid;

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

                    int height = _image.Height,
                        width = _image.Width;

                    Parallel.For(0, height, y =>
                    {
                        byte* sourceRow = (byte*)sourceData.Scan0 + (y * sourceData.Stride);

                        for (int x = 0; x < width; x++)
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
                    });
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
            Uuid = Guid.NewGuid();
            Reset();
        }

        public Darkroom BlackAndWhite(BlackAndWhiteMode mode = BlackAndWhiteMode.Regular)
        {
            appliedFilters.Add(new Filter {
                Name = Filters.BlackAndWhite,
                Value = mode
            });

            return this;
        }

        public Darkroom Invert()
        {
            appliedFilters.Add(new Filter() {
                Name = Filters.Invert
            });
            return this;
        }

        public Darkroom Contrast(double value)
        {
            var lookup = FilterValue.NormalizeContrast(value);

            appliedFilters.Add(new Filter() {
                Name = Filters.Contrast,
                Value = lookup
            });

            return this;
        }

        public Darkroom Brightness(double value)
        {
            value = FilterValue.NormalizeBrightness(value);

            appliedFilters.Add(new Filter()
            {
                Name = Filters.Brightness,
                Value = value
            });

            return this;
        }

        public Darkroom Saturation(double value)
        {
            var lookup = FilterValue.NormalizeSaturation(value);

            appliedFilters.Add(new Filter()
            {
                Name = Filters.Saturation,
                Value = lookup
            });

            return this;
        }

        public Darkroom Vibrance(double value)
        {
            value = FilterValue.NormalizeVibrance(value);

            appliedFilters.Add(new Filter()
            {
                Name = Filters.Vibrance,
                Value = value
            });

            return this;
        }

        public Darkroom Gammma(double value)
        {
            var lookup = FilterValue.NormalizeGamma(value);

            appliedFilters.Add(new Filter()
            {
                Name = Filters.Gamma,
                Value = lookup
            });

            return this;
        }

        public Darkroom Noise(double value)
        {
            value = FilterValue.NormalizeNoise(value);

            appliedFilters.Add(new Filter()
            {
                Name = Filters.Noise,
                Value = value
            });

            return this;
        }

        public Darkroom Sepia(double value = 100)
        {
            value = FilterValue.NormalizeSepia(value);

            appliedFilters.Add(new Filter()
            {
                Name = Filters.Sepia,
                Value = value
            });

            return this;
        }

        public Darkroom Hue(double value)
        {
            value = FilterValue.NormalizeHue(value);

            appliedFilters.Add(new Filter()
            {
                Name = Filters.Hue,
                Value = value
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
            appliedFilters.Add(new Filter()
            {
                Name = Filters.Tint,
                Value = color
            });

            return this;
        }

        public Negative Wash()
        {
            try
            {
                ApplyFilters();
                return _internal;
            }
            finally
            {
                Reset();
            }
        }

        private void ApplyFilters()
        {
            _ProcessPixels((pixel) => {
                foreach(var filter in appliedFilters)
                {
                    switch (filter.Name)
                    {
                        case Filters.BlackAndWhite:
                            pixel = pixel.BlackAndWhite((BlackAndWhiteMode)filter.Value);
                            break;

                        case Filters.Brightness:
                            pixel = pixel.Brightness((double)filter.Value);
                            break;

                        case Filters.Contrast:
                            pixel = pixel.Contrast((byte[])filter.Value);
                            break;

                        case Filters.Gamma:
                            pixel = pixel.Gamma((byte[])filter.Value);
                            break;

                        case Filters.Hue:
                            pixel = pixel.Hue((double)filter.Value);
                            break;

                        case Filters.Invert:
                            pixel = pixel.Invert();
                            break;

                        case Filters.Noise:
                            pixel = pixel.Noise((double)filter.Value);
                            break;

                        case Filters.Saturation:
                            pixel = pixel.Saturation((double[])filter.Value);
                            break;

                        case Filters.Sepia:
                            pixel = pixel.Sepia((double)filter.Value);
                            break;

                        case Filters.Tint:
                            pixel = pixel.Tint((HexColor)filter.Value);
                            break;

                        case Filters.Vibrance:
                            pixel = pixel.Vibrance((double)filter.Value);
                            break;
                    }
                }

                return pixel;
            });
        }

        public Darkroom Reset()
        {
            appliedFilters.Clear();
            _internal = _original.Clone();
            return this;
        }
    }
}
