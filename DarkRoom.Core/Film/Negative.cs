﻿using DarkRoom.Core.Enums;
using DarkRoom.Core.Utils;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DarkRoom.Core.Film
{
    [Serializable]
    public sealed class Negative : IDisposable
    {
        internal Image _image;
 
        private static Bitmap _LoadUri(Uri path)
        {
            try
            {
                Bitmap bmp;
                using (WebClient client = new WebClient())
                {
                    byte[] originalData = client.DownloadData(path);
                    bmp = new Bitmap(new MemoryStream(originalData));
                    originalData = null;
                }
                return bmp;
            }            
            catch(Exception e)
            {
                throw new Exception("Could not load image from url.", e);
            }
        }

        public FileInfo ImageInfo { get; private set; }

        public Negative(string path, int width = 0, int height = 0)
            : this(Image.FromFile(path), width, height)
        {
            ImageInfo = new FileInfo(path);
        }

        public Negative(Uri path, int width = 0, int height = 0) : this(_LoadUri(path), width, height) { }

        public Negative(Bitmap image, int width = 0, int height = 0) : this((Image)image, width, height) { }

        public Negative(Image image, int width = 0, int height = 0)
        {
            if (width != height)
            {
                double ratio = width >= height ? (double)image.Height / image.Width : (double)image.Width / image.Height;
                if (width == 0)
                    width = (int)(height * ratio);
                if (height == 0)
                    height = (int)(width * ratio);
            }

            if (width == 0 && height == 0)
                _image = image;
            else _image = image.GetThumbnailImage(width, height, null, IntPtr.Zero);
        }

        public void Develop(string path, int width = 0, int height = 0)
        {
            string extension = ExtensionHelper.Normalize(path.Split('.').Last());
            try
            {
                Format format = (Format)Enum.Parse(typeof(Format), extension);
                Develop(path, format, width, height);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Unknown image format '{0}' please use a known image format.", extension), ex);
            }
        }

        public void Develop(string path, Format format, int width = 0, int height = 0)
        {
            if(width != height)
            {
                double ratio = width >= height ?  (double)_image.Height / _image.Width : (double)_image.Width / _image.Height;
                if(width == 0)
                    width = (int)(height * ratio);
                if(height == 0)
                    height = (int)(width * ratio);
            }
            var imageFormat = (ImageFormat)typeof(ImageFormat).GetProperty(format.ToString()).GetValue(null, null);

            if (width == 0 && height == 0)
                _image.Save(path, imageFormat);
            else _image.GetThumbnailImage(width, height, null, IntPtr.Zero).Save(path, imageFormat);
        }

        public Negative Cut(int x, int y, int width, int height)
        {
            Bitmap originalBitmap = (Bitmap)_image;

            if(x > originalBitmap.Width)
            {
                throw new Exception(string.Format("Horizontal displacement (x) is greater than the image width. {0} > {1}.", x, originalBitmap.Width));
            }

            if (y > originalBitmap.Height)
            {
                throw new Exception(string.Format("Vertical displacement (y) is greater than the image height. {0} > {1}.", y, originalBitmap.Height));
            }

            if (originalBitmap.Width < width || originalBitmap.Height < height)
            {
                throw new Exception("Cropped height and width cannot be greater than the original image size.");
            }

            BitmapData rawOriginal = originalBitmap.LockBits(new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height), 
                ImageLockMode.ReadOnly, 
                PixelFormat.Format32bppArgb);

            Bitmap croppedBitmap = new Bitmap(width, height);
            BitmapData rawCropped = croppedBitmap.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, 
                PixelFormat.Format32bppArgb);

            const int pixelSize = 4;

            try {
                unsafe
                {
                    Parallel.For(0, height, i =>
                    {
                        byte* sourceRow = (byte*)rawOriginal.Scan0 + ((i + y) * rawOriginal.Stride);
                        byte* destinationRow = (byte*)rawCropped.Scan0 + (i * rawCropped.Stride);

                        for (int j = 0; j < width; j++)
                        {
                            int horizontalDisplacement = (x * pixelSize);

                            destinationRow[j * pixelSize + 0] = sourceRow[horizontalDisplacement + j * pixelSize + 0];
                            destinationRow[j * pixelSize + 1] = sourceRow[horizontalDisplacement + j * pixelSize + 1];
                            destinationRow[j * pixelSize + 2] = sourceRow[horizontalDisplacement + j * pixelSize + 2];
                            destinationRow[j * pixelSize + 3] = sourceRow[horizontalDisplacement + j * pixelSize + 3];
                        }
                    });
                }
            }
            finally
            {
                if(rawOriginal != null)
                    originalBitmap.UnlockBits(rawOriginal);
                if(rawCropped != null)
                    croppedBitmap.UnlockBits(rawCropped);
            }

            GC.Collect();
            _image = croppedBitmap;
            return this;
        }

        public DataUri Digitize()
        {
            MemoryStream ms = new MemoryStream();
            _image.Save(ms, ImageFormat.Png);
            byte[] byteImage = ms.ToArray();

            return new DataUri()
            {
                Data = Convert.ToBase64String(byteImage),
                Mime = Format.Png,
                Encoding = "base64"
            };
        }

        public int Width
        {
            get
            {
                return _image.Width;
            }
        }

        public int Height
        {
            get
            {
                return _image.Height;
            }
        }

        public static bool operator == (Negative left, Negative right)
        {
            if (left.Equals(right))
                return true;

            if (left.Height != right.Height || left.Width != right.Width)
                return false;

            Bitmap source = (Bitmap)left._image,
                   target = (Bitmap)right._image;

            const int pixelSize = 4; // 32 bits per pixel

            BitmapData sourceData = null, 
                       targetData = null;
            unsafe
            {
                try
                {
                    sourceData = source.LockBits(
                      new Rectangle(0, 0, source.Width, source.Height),
                      ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                    targetData = target.LockBits(
                      new Rectangle(0, 0, target.Width, target.Height),
                      ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                    for (int y = 0; y < source.Height; ++y)
                    {
                        byte* sourceRow = (byte*)sourceData.Scan0 + (y * sourceData.Stride);
                        byte* targetRow = (byte*)targetData.Scan0 + (y * targetData.Stride);

                        for (int x = 0; x < source.Width; ++x)
                        {
                            if (targetRow[x * pixelSize + 0] != sourceRow[x * pixelSize + 0])
                                return false;
                            if (targetRow[x * pixelSize + 1] != sourceRow[x * pixelSize + 1])
                                return false;
                            if (targetRow[x * pixelSize + 2] != sourceRow[x * pixelSize + 2])
                                return false;
                            if (targetRow[x * pixelSize + 3] != sourceRow[x * pixelSize + 3])
                                return false;
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

            return true;
        }

        public static bool operator != (Negative left, Negative right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return this.GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
           return _image.GetHashCode();
        }

        public void Dispose()
        {
            _image.Dispose();
        }
    }
}
