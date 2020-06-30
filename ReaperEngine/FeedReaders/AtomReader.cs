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
    /// Parses an XmlDocument and provides Atom reading features to the scanner.
    /// </summary>
    class AtomReader
    {
        #region members
        private Source _source;
        private AppState _appState;
        #endregion

        #region accessors
        /// <summary>
        /// The Source containing the xml being used as the Atom feed.
        /// </summary>
        public Source Source { get { return this._source; } }
        #endregion

        #region constructors
        /// <summary>
        /// Instantiates a new AtomReader object.
        /// </summary>
        internal AtomReader(Source source, AppState appState)
        {
            if (source == null)
                throw new ArgumentNullException();

            this._source = source;
            this._appState = appState;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Converts all Atom items into local Item objects.
        /// </summary>
        public void GetFeedItems()
        {
            // the two versions aren't parsable inline together.
            if (this._source.FeedFormat == FeedFormat.Atom1Point0)
                this.Get1Point0Feeds();
            else if (this._source.FeedFormat == FeedFormat.Atom0Point3)
                this.Get0Point3Feeds();
        }
        #endregion

        #region private methods
        /// <summary>
        /// Converts all Atom v1.0 items into local Item objects.
        /// </summary>
        private void Get1Point0Feeds()
        {
            DateTime feedUpdatedTime = DateTime.MinValue;
            GenericCollection<string> feedCategories = null;

            // custom xml namespaces.
            XmlNamespaceManager xmlns = new XmlNamespaceManager(this.Source.LatestFeedDocument.NameTable);
            xmlns.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");

            // channel information.
            XmlNode feedNode = this.Source.LatestFeedDocument.SelectSingleNode("feed");
            if (feedNode != null)
            {
                // content time-stamp.
                XmlNode feedUpdatedNode = feedNode.SelectSingleNode("updated");
                if (feedUpdatedNode != null && Common.IsDate(feedUpdatedNode.InnerText))
                    feedUpdatedTime = DateTime.Parse(feedUpdatedNode.Value);

                // categories.
                feedCategories = new GenericCollection<string>();
                foreach (XmlNode categoryNode in feedNode.SelectNodes("category"))
                    feedCategories.Add(categoryNode.InnerText.Trim());
            }

            // entries.
            foreach (XmlNode entry in this.Source.LatestFeedDocument.SelectNodes("entry"))
            {
                try
                {
                    Item item = new Item();
                    item.ImportTime = DateTime.Now;

                    // required Atom elements.
                    item.Title = entry.SelectSingleNode("title").InnerText.Trim();
                    item.Description = entry.SelectSingleNode("summary").InnerText.Trim();
                    item.ContentUrl = new Uri(entry.SelectSingleNode("link[@href]").Attributes["href"].Value.Trim());

                    // optional Atom elements.
                    // -------------------------------------------------------------------------

                    XmlNode itemPubDateNode = entry.SelectSingleNode("published");
                    if (itemPubDateNode != null && Common.IsDate(itemPubDateNode.InnerText))
                        item.PublicationTime = DateTime.Parse(itemPubDateNode.InnerText);

                    XmlNode categoryNode = entry.SelectSingleNode("category");
                    if (categoryNode != null)
                        item.FeedSpecificData.Categories.Add(categoryNode.InnerText.Trim().ToLower());

                    XmlNode categoryNode2 = entry.SelectSingleNode("dc:subject");
                    if (categoryNode2 != null)
                        item.FeedSpecificData.Categories.Add(categoryNode2.InnerText.Trim().ToLower());

                    XmlNode guidNode = entry.SelectSingleNode("id");
                    if (guidNode != null)
                        item.FeedSpecificData.Guid = guidNode.InnerText.Trim().ToLower();

                    foreach (XmlNode enclosureNode in entry.SelectNodes("link[@rel='enclosure']"))
                    {
                        XmlElement enclosureElement = enclosureNode as XmlElement;
                        if (Common.IsMimeTypeWebImage(enclosureElement.GetAttribute("type")))
                            item.FeedSpecificData.ImageUrls.Add(new Uri(enclosureElement.GetAttribute("href")));
                    }

                    // -------------------------------------------------------------------------

                    // processing.
                    if (item.FeedSpecificData.Categories.Count == 0 && feedCategories.Count > 0)
                        item.FeedSpecificData.Categories = feedCategories;

                    // publication time.
                    if (item.PublicationTime == DateTime.MinValue)
                    {
                        // no item pubDate specified, try using channel times, failing that, use now.
                        if (feedUpdatedTime != DateTime.MinValue)
                            item.PublicationTime = feedUpdatedTime;
                        else
                            item.PublicationTime = DateTime.Now;
                    }

                    // validation.
                    if (item.Title == String.Empty || item.Description == String.Empty || item.ContentUrl == null)
                        continue;

                    // done.
                    _source.LatestItems.Add(item);
                }
                catch
                {
                    this._appState.Logger.LogError("error", string.Format("RDF parsing failed for: {0}", Source.FeedUrl));
                }
            }
        }

        /// <summary>
        /// Converts all Atom v0.3 items into local Item objects.
        /// </summary>
        private void Get0Point3Feeds()
        {
            DateTime feedUpdatedTime = DateTime.MinValue;
            
            // custom xml namespaces.
            XmlNamespaceManager xmlns = new XmlNamespaceManager(this._source.LatestFeedDocument.NameTable);
            xmlns.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");

            // channel information.
            XmlNode feedNode = this._source.LatestFeedDocument.SelectSingleNode("feed");
            if (feedNode != null)
            {
                // content time-stamp.
                XmlNode feedUpdatedNode = feedNode.SelectSingleNode("modified");
                if (feedUpdatedNode != null && Common.IsDate(feedUpdatedNode.InnerText))
                    feedUpdatedTime = DateTime.Parse(feedUpdatedNode.Value);
            }
 
            // entries.
            //foreach (XmlNode entry in feedNode.SelectNodes("entry"))
            foreach (XmlNode entry in this.Source.LatestFeedDocument.SelectNodes("entry"))
            {
                Item item = new Item();

                // required Atom elements.
                item.Title = entry.SelectSingleNode("title").InnerText.Trim();
                item.ContentUrl = new Uri(entry.SelectSingleNode("link[@rel='alternate']").Attributes["href"].Value.Trim());

                XmlNode summaryNode = entry.SelectSingleNode("summary");
                if (summaryNode != null)
                    item.Description = summaryNode.InnerText.Trim();
                else
                    item.Description = entry.SelectSingleNode("content").InnerText.Trim();

                // optional Atom elements.
                // -------------------------------------------------------------------------

                XmlNode itemPubDateNode = entry.SelectSingleNode("issued");
                if (itemPubDateNode != null && Common.IsDate(itemPubDateNode.InnerText))
                    item.PublicationTime = DateTime.Parse(itemPubDateNode.InnerText);

                XmlNode categoryNode = entry.SelectSingleNode("dc:subject");
                if (categoryNode != null)
                    item.FeedSpecificData.Categories.Add(categoryNode.InnerText.Trim().ToLower());

                XmlNode guidNode = entry.SelectSingleNode("id");
                if (guidNode != null)
                    item.FeedSpecificData.Guid = guidNode.InnerText.Trim().ToLower();

                // -------------------------------------------------------------------------

                // publication time.
                if (item.PublicationTime == DateTime.MinValue)
                {
                    // no item pubDate specified, try using channel times, failing that, use now.
                    if (feedUpdatedTime != DateTime.MinValue)
                        item.PublicationTime = feedUpdatedTime;
                    else
                        item.PublicationTime = DateTime.Now;
                }

                // validation.
                if (item.Title == String.Empty || item.Description == String.Empty || item.ContentUrl == null)
                    continue;

                // done.
                _source.LatestItems.Add(item);
            }
        }
        #endregion
    }
}