using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPanther.Framework.Web
{
    public class Web
    {
        /// <summary>
        /// Returns the page segment of a URL.
        /// </summary>
        public static string PageNameFromUrl(string path)
        {
            return path.Substring((path.LastIndexOf("/") + 1));
        }
    }
}