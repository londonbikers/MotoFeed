using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using MediaPanther.Aggregator;
using MediaPanther.Aggregator.Items;
using MediaPanther.Aggregator.Categories;
using MediaPanther.Aggregator.Sources;
using MediaPanther.Framework;
using MediaPanther.Framework.Files;
using MediaPanther.Framework.Web;
using MediaPanther.Framework.RegularExpressions;
using MediaPanther.Framework.Content;

namespace MediaPanther.Aggregator.Reaper
{
    /// <summary>
    /// Handles all processes regarding importing new items to the content-store.
    /// </summary>
    class Importer
    {
        #region members
        private Source _source;
        private AppState _appState;
        private GenericCollection<Category> _categories;
        #endregion

        #region accessors
        /// <summary>
        /// The Source this Importer is working with.
        /// </summary>
        public Source Source { get { return this._source; } }
        #endregion

        #region contructors
        /// <summary>
        /// Instantiates a new Importer for a particular Source object.
        /// </summary>
        public Importer(AppState appState, Source sourceToImportFor)
        {
            this._appState = appState;
            this._source = sourceToImportFor;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Begins the process of importing all new items from a Source.
        /// </summary>
        public void ImportNewItems()
        {
            //1: determine new items.
            //2: categorise items
            //3: import new items
            //4: client notifications?

            // .........................

            this._categories = Server.Instance.CategoryServer.GetAllCategories();

            // .........................

            if (this.MarkNewItems() > 0)
            {
                foreach (Item item in this._source.LatestItems)
                {
                    if (item.FeedSpecificData.IsNewToUs)
                    {
                        this.CategoriseItem(item);
                        this.DownloadItemCoverImage(item);
                        this.PersistItemToContentStore(item);
                    }
                }
            }
        }
        #endregion

        #region private methods
        /// <summary>
        /// Works out which, if any, are new items to us, from the Source.
        /// </summary>
        /// <returns>How many new items there are.</returns>
        private int MarkNewItems()
        {
            int newItemCount = 0;
            SqlConnection connection = this._appState.GetDatabaseConnection();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "Reaper_IsItemNew";
            SqlParameter linkParam = new SqlParameter("@Link", String.Empty);
            command.Parameters.Add(linkParam);

            try
            {
                foreach (Item item in _source.LatestItems)
                {
                    linkParam.Value = item.ContentUrl.AbsoluteUri;
                    item.FeedSpecificData.IsNewToUs = (bool)command.ExecuteScalar();
                    if (item.FeedSpecificData.IsNewToUs)
                        newItemCount++;
                }
            }
            catch (Exception ex)
            {
                this._appState.Logger.LogError("error", "ImportNewFeedItems() newness check failed: " + ex.Message);
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }

            return newItemCount;
        }

        /// <summary>
        /// Attempts to categorise the item by assigning tags and categories.
        /// </summary>
        private void CategoriseItem(Item itemToCategorise)
        {
            //-- run through all categories and their tags
            //-- search for tag matches in content
            //-- keep track of category with most matches

            int lastWordMatches = 0;
            int lastTotalMatches = 0;
            int currentWordMatches = 0;
            int currentTotalMatches = 0;
            //int bestMatchWordMatches = 0;
            //int bestMatchTotalMatches = 0;
            Category bestMatchCategory = null;

            foreach (Category cat in this._categories)
            {
                currentTotalMatches = 0;
                currentWordMatches = 0;

                foreach (Tag tag in cat.Tags)
                {
                    // how many (if any) matches of this tag are there in the source content?
                    string analysisContent = itemToCategorise.Title.ToLower() + " " + itemToCategorise.Description.ToLower();
                    MatchCollection matches = Regex.Matches(analysisContent, "\\b" + tag.Name + "\\b", RegexOptions.IgnoreCase);

                    if (matches.Count > 0)
                    {
                        currentWordMatches++;
                        currentTotalMatches += matches.Count;

                        // add the tag to the item, as it'll be valuable to the item outside of this task.
                        itemToCategorise.Tags.Add(tag);
                    }
                }

                // tally-up category results.
                if (currentWordMatches >= lastWordMatches)
                {
                    // best match if more word matches, else if same word matches use the total matches to decide the draw.
                    if (currentWordMatches > lastWordMatches || (currentWordMatches == lastWordMatches && currentTotalMatches > lastTotalMatches))
                    {
                        bestMatchCategory = cat;
                        lastWordMatches = currentWordMatches;
                        lastTotalMatches = currentTotalMatches;
                    }
                }
            }

            if (bestMatchCategory != null)
            {
                itemToCategorise.Category = bestMatchCategory;
            }
            else
            {
                // uncategorised.
                foreach (Category cat in this._categories)
                {
                    if (cat.Name == "General")
                    {
                        itemToCategorise.Category = cat;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Once fully populated and categorised, the item can now be persisted to the content-store.
        /// </summary>
        private void PersistItemToContentStore(Item itemToPersist)
        {
            itemToPersist.Description = Content.HtmlStripTags(itemToPersist.Description, false, false);
            Server.Instance.ItemServer.UpdateItem(itemToPersist);
        }

        /// <summary>
        /// Scans the item content for any cover images that can be downloaded and gets them, storing
        /// them in the local media cache.
        /// </summary>
        private void DownloadItemCoverImage(Item itemToGetImageFor)
        {
            // if there's more than one image we need to pick the largest one, which is hopefully the best one. There's lots of small branding images in feeds to skip.
            string url = this.GetBestImageUrlFromPossibles(itemToGetImageFor);
            if (url == String.Empty)
                return;

            // eg: "c:\media\i\2007\10\29\rossi-wins-valencia.jpg".
            string filePath = Properties.Settings.Default.MediaCachePath + string.Format(@"i\{0}\{1}\{2}\", itemToGetImageFor.ImportTime.Year, itemToGetImageFor.ImportTime.Month, itemToGetImageFor.ImportTime.Day);
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            // -------------------------------------------------------------------

            Stream stream = null;
            FileStream fstream = null;
            bool downloadSuccessful = false;
            HttpWebResponse response = null;
            string localCacheImagePath = String.Empty;

            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Timeout = 10000;
                response = request.GetResponse() as HttpWebResponse;
                
                // inspect the response to see what we're getting back. don't accept invalid images.
                if (response.ContentLength > Properties.Settings.Default.MinimumImageFilesizeBytes && Common.IsMimeTypeWebImage(response.ContentType))
                {
                    // resolve the filename.
                    localCacheImagePath = this.ResolveImageFilename(filePath, url, response.ContentType);
                    
                    stream = response.GetResponseStream();
                    byte[] inBuffer = this.ReadFully(stream, 32768);
                    fstream = new FileStream(localCacheImagePath, FileMode.OpenOrCreate, FileAccess.Write);
                    fstream.Write(inBuffer, 0, inBuffer.Length - 1);
                    downloadSuccessful = true;
                }
                //else
                //{
                //    if (this._appState.Config.InDebugMode)
                //        this._appState.Logger.LogInfo("debug", string.Format("Unhandled image content-type received: {0}", response.ContentType));
                //}
            }
            catch (Exception ex)
            {
                if (this._appState.Config.InDebugMode)
                {
                    this._appState.Logger.LogInfo("debug", string.Format("Item media download failed for: {0}, item url: {1}.", url, itemToGetImageFor.ContentUrl.AbsoluteUri));
                    this._appState.Logger.LogInfo(ex.Message);
                }
            }
            
            if (fstream != null)
                fstream.Close();
            
            if (stream != null)
                stream.Close();

            if (response != null)
                response.Close();

            if (downloadSuccessful)
                itemToGetImageFor.ImageFilename = Path.GetFileName(localCacheImagePath);
        }

        /// <summary>
        /// Reads data from a stream until the end is reached. The
        /// data is returned as a byte array. An IOException is
        /// thrown if any of the underlying IO calls fail.
        /// </summary>
        /// <param name="stream">The stream to read data from</param>
        /// <param name="initialLength">The initial buffer length</param>
        public byte[] ReadFully(Stream stream, int initialLength)
        {
            /* If we've been passed an unhelpful initial length, just
             use 32K. */
            if (initialLength < 1)
                initialLength = 32768;

            byte[] buffer = new byte[initialLength];
            int read = 0;
            int chunk;

            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                /* If we've reached the end of our buffer, check to see if there's
                 any more information */
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();

                    /* End of stream? If so, we're done */
                    if (nextByte == -1)
                        return buffer;

                    /* Nope. Resize the buffer, put in the byte we've just read, and continue */
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }

            /* Buffer is now too big. Shrink it. */
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }

        /// <summary>
        /// Enumerates a collection of url's from a RegEx MatchCollection to look for the best match image, i.e. skipping small branding images.
        /// </summary>
        public string GetBestImageUrlFromPossibles(Item item)
        {
            long bestFileSize = 0;
            string bestURL = String.Empty;
            ArrayList urls = new ArrayList();

            // are there any declared image urls?
            if (item.FeedSpecificData.ImageUrls.Count > 0)
            {
                foreach (Uri uri in item.FeedSpecificData.ImageUrls)
                    urls.Add(uri.AbsoluteUri);
            }

            // look for urls embedded in the item description.
            urls.AddRange(RegularExpressions.FindAllImageUrls(item.Description));
            
            foreach (string url in urls)
            {
                HttpWebRequest request = null;
                try
                {
                    request = WebRequest.Create(url) as HttpWebRequest;
                    //request.Timeout = 10000;
                    request.Method = "HEAD";
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                    if (response.ContentLength > Properties.Settings.Default.MinimumImageFilesizeBytes && Common.IsMimeTypeWebImage(response.ContentType))
                    {
                        if (response.ContentLength > bestFileSize)
                        {
                            bestFileSize = response.ContentLength;
                            bestURL = url;
                        }
                    }

                    request = null;
                    response.Close();
                    response = null;
                }
                catch
                {
                    // probably a duff url.
                    if (Properties.Settings.Default.InDebugMode)
                        this._appState.Logger.LogDebug("debug", string.Format("Couldn't request image url: {0}", url));
                }
            }

            return bestURL;
        }
        #endregion

        #region private methods
        /// <summary>
        /// Ensures a unique, correct and clean filename is used to store an image in the media cache.
        /// </summary>
        /// <param name="path">The local path to where the image will be saved to.</param>
        /// <param name="url">The image source full url. i.e. 'http://domain.com/image.jpg'.</param>
        /// <param name="mimeType">The file mime-type for the image, i.e. 'image/jpg'.</param>
        /// <returns>An absolute local path, i.e. 'c:\media\i\2007\12\1\image.jpg'</returns>
        private string ResolveImageFilename(string path, string url, string mimeType)
        {
            // -- format dynamic image url.
            // -- clean filename.
            // -- add any missing extension/

            string filename = Web.PageNameFromUrl(url);

            // irregular image filename?
            if (!Files.IsFilenameAnImage(filename))
            {
                filename = RegularExpressions.RemoveNonAlphaNumericCharacters(filename);
                filename += Files.GetFileExtensionFromMimeType(mimeType);
            }

            filename = Files.GetSafeFilename(filename);
            filename = Files.GetUniqueFilename(path, filename);
            path = Path.Combine(path, filename);
            return path;
        }
        #endregion
    }
}