using DarkRoom.Core.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DarkRoom.Core.Film
{
    [Serializable]
    public class Negative
    {
        internal Image _image;
        private string _ParseExtension(string path)
        {
            string extension = path.Split('.').LastOrDefault();

            if (string.IsNullOrWhiteSpace(extension))
                throw new Exception("Image file extension is required in order to infer the saved format required. Please specify the image format manually if the desired image needs to be saved without a file extension.");

            extension = extension.Trim().ToLower();

            var charArray = extension.ToCharArray();

            charArray[0] = (char)(extension[0] - 32);

            return new string(charArray);
        }
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

        public Negative(string path) : this(Image.FromFile(path))
        {
            ImageInfo = new FileInfo(path);
        }

        public Negative(Uri path) : this(_LoadUri(path)) { }

        public Negative(Bitmap image) : this((Image)image) { }

        public Negative(Image image)
        {
            _image = image;
        }

        public void Develop(string path, int width = 0, int height = 0)
        {
            string extension = _ParseExtension(path);
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

        public static bool operator !=(Negative left, Negative right)
        {
            return !(left == right);
        }
    }
}
