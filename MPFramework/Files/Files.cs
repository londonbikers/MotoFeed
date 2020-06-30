using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MediaPanther.Framework.Files
{
    public class Files
    {
        /// <summary>
        /// This function checks to see if there's a file already in the supplied path
        /// with the filename, if one's found, it'll adapt it to make sure it's unique.
        /// </summary>
        public static string GetUniqueFilename(string path, string filename)
        {
            filename = filename.Trim();
            bool isUnique = false;
            string filenamePart = Path.GetFileNameWithoutExtension(filename);
            string extension = Path.GetExtension(filename);

            if (!path.EndsWith("\\"))
                path += "\\";

            while (!isUnique)
            {
                if (File.Exists(path + filename))
                {
                    filenamePart += string.Format("-{0}", DateTime.Now.Second);
                    filename = filenamePart + extension;
                }
                else
                {
                    isUnique = true;
                }
            }

            return filename;
        }

        /// <summary>
        /// Transforms a filename so that it doesn't contain any illegal characters.
        /// </summary>
        /// <param name="filename">The filename to transform.</param>
        public static string GetSafeFilename(string filename)
        {
            string name = Path.GetFileNameWithoutExtension(filename);
            string extension = Path.GetExtension(filename);

            // hypens and spaces are acceptable, so let's just translate them for now.
            name = name.Replace("%20", " ");
            name = name.Replace(" ", "-");
            name = name.Replace("-", "ZZHYPZZ");
            name = name.Replace("_", "ZZHYPZZ");
            name = name.Replace(extension, String.Empty);
            name = RegularExpressions.RegularExpressions.RemoveNonAlphaNumericCharacters(name);
            name = name.Replace("ZZHYPZZ", "-");

            return name + extension;
        }

        /// <summary>
        /// Attempts to translate a mime-type into a file extension. Currently support os image types only.
        /// </summary>
        /// <param name="mimeType">The mime-type signature, i.e. 'image/jpeg'.</param>
        /// <returns>An extension including the period, or an empty string if translation not possible.</returns>
        public static string GetFileExtensionFromMimeType(string mimeType)
        {
            if (mimeType == String.Empty)
                return String.Empty;

            string extension = String.Empty;
            mimeType = mimeType.ToLower().Trim();
            if (mimeType == "image/jpeg" || mimeType == "image/pjpeg")
                extension = ".jpg";
            else if (mimeType == "image/gif")
                extension = ".gif";
            else if (mimeType == "image/png")
                extension = ".png";
            else if (mimeType == "image/bmp")
                extension = ".bmp";

            return extension;
        }

        /// <summary>
        /// Determines if a filename has a traditional image signature, i.e. ends with an image file extension.
        /// </summary>
        /// <param name="filename">The filename or path and filename to inspect.</param>
        public static bool IsFilenameAnImage(string filename)
        {
            filename = filename.ToLower().Trim();
            if (filename.EndsWith(".jpg") || filename.EndsWith(".jpeg") || filename.EndsWith(".gif") || filename.EndsWith(".png") || filename.EndsWith(".bmp"))
                return true;
            else
                return false;
        }
    }
}
