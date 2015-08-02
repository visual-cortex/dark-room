using DarkRoom.Core.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRoom.Core.Models
{
    public class Negative
    {
        public Image Image { get; private set; }
        public FileInfo ImageInfo { get; set; }

        public Negative(Bitmap image) : this((Image)image) { }

        public Negative(string path) : this(Image.FromFile(path)) {
            ImageInfo = new FileInfo(path);
        }

        public Negative(Image image)
        {
            Image = image;
        }

        public void Save(string path, Format format = Format.Jpeg)
        {
            var imageFormat = typeof(ImageFormat);

            Image.Save(path, (ImageFormat)imageFormat.GetProperty(format.ToString()).GetValue(format.ToString()));
        }
    }
}
