using System;
using System.Xml;
using System.Collections.Generic;
using MediaPanther.Aggregator;
using MediaPanther.Aggregator.Items;
using MediaPanther.Aggregator.Sources;
using MediaPanther.Framework;

namespace MediaPanther.Aggregator.Reaper
{
    /// <summary>
    /// Parses an XmlDocument and provides RSS reading features to the scanner.
    /// </summary>
    class RssReader
    {
        #region members
        private Source _source;
        private AppState _appState;
        #endregion

        #region accessors
        /// <summary>
        /// The Source containing the xml being used as the RSS feed.
        /// </summary>
        public Source Source { get { return this._source; } }
        #endregion

        #region constructors
        /// <summary>
        /// Instantiates a new RssReader object.
        /// </summary>
        internal RssReader(Source source, AppState appState)
        {
            if (source == null)
                throw new ArgumentNullException();

            this._source = source;
            this._appState = appState;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Converts all RSS items into local Item objects.
        /// </summary>
        public void GetFeedItems()
        {
            DateTime channelPubDate = DateTime.MinValue;
            DateTime channelLastBuildDate = DateTime.MinValue;

            // channel information.
            XmlNode channelNode = _source.LatestFeedDocument.SelectSingleNode("rss/channel");
            if (channelNode == null)
                throw new Exceptions.FeedContentsException("No Channel node found! (" + Source.FeedUrl.AbsoluteUri + ")");

            // content time-stamps.
            XmlNode pubDateNode = channelNode.SelectSingleNode("pubDate");
            if (pubDateNode != null && Common.IsDate(pubDateNode.InnerText))
                channelPubDate = DateTime.Parse(pubDateNode.InnerText);

            XmlNode lastBuildDateNode = channelNode.SelectSingleNode("lastBuildDate");
            if (lastBuildDateNode != null && Common.IsDate(lastBuildDateNode.InnerText))
                channelLastBuildDate = DateTime.Parse(lastBuildDateNode.InnerText);

            // categories.
            GenericCollection<string> channelCategories = new GenericCollection<string>();
            foreach (XmlNode categoryNode in channelNode.SelectNodes("category"))
                channelCategories.Add(categoryNode.InnerText.Trim());

            // items.
            foreach (XmlNode entry in channelNode.SelectNodes("item"))
            {
                try
                {
                    Item item = new Item();
                    item.ImportTime = DateTime.Now;

                    // required RSS elements.
                    if (entry.SelectSingleNode("title") != null)
                        item.Title = entry.SelectSingleNode("title").InnerText.Trim();
                    else
                        item.Title = this.Source.Name + ": Untitled";

                    item.ContentUrl = new Uri(entry.SelectSingleNode("link").InnerText.Trim());

                    // description is only required in 2.0.
                    if (Source.FeedFormat == FeedFormat.Rss2Point0)
                        item.Description = entry.SelectSingleNode("description").InnerText.Trim();

                    // optional RSS elements.
                    // -------------------------------------------------------------------------

                    XmlNode itemPubDateNode = entry.SelectSingleNode("pubDate");
                    if (itemPubDateNode != null && Common.IsDate(itemPubDateNode.InnerText))
                        item.PublicationTime = DateTime.Parse(itemPubDateNode.InnerText);

                    XmlNode categoryNode = entry.SelectSingleNode("category");
                    if (categoryNode != null)
                        item.FeedSpecificData.Categories.Add(categoryNode.InnerText.Trim().ToLower());

                    XmlNode guidNode = entry.SelectSingleNode("guid");
                    if (guidNode != null)
                        item.FeedSpecificData.Guid = guidNode.InnerText.Trim().ToLower();

                    foreach (XmlNode enclosureNode in entry.SelectNodes("enclosure"))
                    {
                        XmlElement enclosureElement = enclosureNode as XmlElement;
                        if (Common.IsMimeTypeWebImage(enclosureElement.GetAttribute("type")))
                            item.FeedSpecificData.ImageUrls.Add(new Uri(enclosureElement.GetAttribute("url")));
                    }

                    // -------------------------------------------------------------------------

                    // processing.
                    if (item.FeedSpecificData.Categories.Count == 0 && channelCategories.Count > 0)
                        item.FeedSpecificData.Categories = channelCategories;

                    // publication time.
                    if (item.PublicationTime == DateTime.MinValue)
                    {
                        // no item pubDate specified, try using channel times, failing that, use now.
                        if (channelPubDate != DateTime.MinValue)
                            item.PublicationTime = channelPubDate;
                        else if (channelLastBuildDate != DateTime.MinValue)
                            item.PublicationTime = channelLastBuildDate;
                        else
                            item.PublicationTime = DateTime.Now;
                    }

                    // validation.
                    if (item.Title == String.Empty || item.Description == String.Empty || item.ContentUrl == null)
                        continue;

                    // done.
                    item.Source = this._source;
                    this._source.LatestItems.Add(item);
                }
                catch
                {
                    this._appState.Logger.LogError("error", string.Format("RSS parsing failed for: {0}", Source.FeedUrl));
                }
            }
        }
        #endregion
    }
}