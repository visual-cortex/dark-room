using DarkRoom.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkRoom.Core.Utils
{
    internal static class ExtensionHelper
    {
        internal static string Normalize(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                throw new Exception("Image file extension is required in order to infer the saved format required. Please specify the image format manually if the desired image needs to be saved without a file extension.");

            extension = extension.Trim().ToLower();

            var charArray = extension.ToCharArray();

            charArray[0] = (char)(extension[0] - 32);

            return new string(charArray);
        }
    }
}
