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
    public class Negative
    {
        private Image _image;
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
                throw new Exception(string.Format("Unknown image format '{0}' please use a known image format.", extension));
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
    }
}
