using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DownloadBingImage
{
    public class BingImages
    {
        private const string DOWNLOAD_PATH = @"D:\BingImages";
        private const string BING = "http://www.bing.com";
        private const string IMG_URL = "http://www.bing.com/HPImageArchive" +
                                       ".aspx?format=xml&idx=0&n={0}&mkt={1}";
        private static string[] Markets = new string[]
            {
                "en-US", "zh-CN",
                "ja-JP", "en-AU"
            };
        private const int NUMBER_OF_IMAGES = 1;
        private static IList<string> fileNames = new List<string>();
        /// <summary>
        /// Download images from Bing
        /// </summary>
        public static IList<string> DownLoadImages()
        {
            // Make sure destination folder exists
            ValidateDownloadPath();
            XDocument doc = null;
            WebRequest request = null;
            // Because each market can have different images
            // cycle through each of them
            foreach (string market in Markets)
            {
                // Form the URL based on market
                // Since this will be run once per day only 1 image needs to
                // be downloaded
                string url = string.Format(IMG_URL, NUMBER_OF_IMAGES, market);
                request = WebRequest.Create(url);
                using (Stream stream = request.GetResponse().GetResponseStream())
                {
                    // Load the stream into and XDocument for processing
                    doc = XDocument.Load(stream);
                }

                // Iterate through the image elements
                foreach (XElement image in doc.Descendants("image"))
                {
                    SaveImage(image.Element("url").Value);
                }
            }

            return fileNames;
        }

        /// <summary>
        /// Save image from the give URL to disk
        /// </summary>
        /// <param name="url">URL of image to save</param>
        private static void SaveImage(string url)
        {
            // Images can be duplicated between markets
            // so to avoid duplicates from being downloaded
            // get the unique name based on the image number in the URL
            string filename = GetImageName(url);
            if (!File.Exists(filename))
            {
                // URL is relative so form the absolute URL
                WebRequest request = WebRequest.Create(BING + url);
                using (Stream stream = request.GetResponse().GetResponseStream())
                {
                    Image img = Image.FromStream(stream);
                    img.Save(filename);
                    //fileNames.Add(filename);
                }
            }
        }

        /// <summary>
        /// Create filename for image based on URL
        /// </summary>
        /// <param name="url">Image URL</param>
        /// <returns>FQN for saving image to</returns>
        private static string GetImageName(string url)
        {
            // URL is in this format /fd/hpk2/DiskoBay_EN-US1415620951.jpg
            // Extract the image number
            Regex reg = new Regex(@"[0-9]+\w");
            Match m = reg.Match(url);

            // Should now have 1415620951 from above example
            // Create path to save image to
            return string.Format(@"{0}\{1}", DOWNLOAD_PATH, url.Split('%')[1]);
        }

        /// <summary>
        /// Check if download path exist and create if necessary
        /// </summary>
        private static void ValidateDownloadPath()
        {
            if (!Directory.Exists(DOWNLOAD_PATH))
            {
                Directory.CreateDirectory(DOWNLOAD_PATH);
            }
        }
    }
}
