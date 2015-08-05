using DarkRoom.Core.Enums;
using DarkRoom.Core.Utils;
using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace DarkRoom.Core.Film
{
    public class DataUri
    {
        public DataUri()
        {
            Encoding = "base64";
        }

        public DataUri(string dataUri)
        {
            var regex = new Regex(@"data:(?<mime>[\w/\-\.]+);(?<encoding>\w+),(?<data>.*)", RegexOptions.Compiled);

            if (regex.IsMatch(dataUri))
            {
                var match = regex.Match(dataUri);
                Format format;
                string extension = match.Groups["mime"].Value.Split('/')[1];
                if (Enum.TryParse<Format>(ExtensionHelper.Normalize(extension), out format))
                {
                    Mime = format;
                    Encoding = match.Groups["encoding"].Value;
                    Data = match.Groups["data"].Value;
                }
                else
                    throw new Exception(string.Format("Unknown image format '{0}' please use a known image format.", match.Groups["mime"]));

            }
        }

        public Format Mime { get; set; }
        public string Encoding { get; set; }
        public string Data { get; set; }

        public Negative ToNegative()
        {
            byte[] byteBuffer = Convert.FromBase64String(Data);
            MemoryStream memoryStream = new MemoryStream(byteBuffer);
            memoryStream.Position = 0;

            Bitmap bmp = new Bitmap(memoryStream);

            memoryStream.Close();
            memoryStream = null;
            byteBuffer = null;

            return new Negative(bmp);
        }

        public override string ToString()
        {
            return string.Format("data:image/{0};{1},{2}", Mime.ToString().ToLower(), Encoding, Data);
        }
    }
}
