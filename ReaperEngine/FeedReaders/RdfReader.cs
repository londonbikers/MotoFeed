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
    /// Parses an XmlDocument and provides RDF reading features to the scanner.
    /// </summary>
    class RdfReader
    {
        #region members
        private Source _source;
        private AppState _appState;
        #endregion

        #region accessors
        /// <summary>
        /// The Source containing the xml being used as the RDF feed.
        /// </summary>
        public Source Source { get { return this._source; } }
        #endregion

        #region constructors
        /// <summary>
        /// Instantiates a new RdfReader object.
        /// </summary>
        internal RdfReader(Source source, AppState appState)
        {
            if (source == null)
                throw new ArgumentNullException();

            this._source = source;
            this._appState = appState;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Converts all RDF items into local Item objects.
        /// </summary>
        public void GetFeedItems()
        {
            DateTime channelPubDate = DateTime.MinValue;
            DateTime channelLastBuildDate = DateTime.MinValue;

            // custom xml namespaces.
            XmlNamespaceManager xmlns = new XmlNamespaceManager(this._source.LatestFeedDocument.NameTable);

            xmlns.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            xmlns.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            xmlns.AddNamespace("rss", "http://purl.org/rss/1.0/");

            // channel information.
            XmlNode channelNode = _source.LatestFeedDocument.SelectSingleNode("/rdf:RDF/rss:channel", xmlns);
            if (channelNode == null)
                throw new Exceptions.FeedContentsException("No Channel node found! (" + Source.FeedUrl.AbsoluteUri + ")");

            // content time-stamps.
            XmlNode pubDateNode = channelNode.SelectSingleNode("dc:date", xmlns);
            if (pubDateNode != null && Common.IsDate(pubDateNode.InnerText))
                channelPubDate = DateTime.Parse(pubDateNode.InnerText);

            // items.
            foreach (XmlNode entry in channelNode.SelectNodes("/rdf:RDF/rss:item", xmlns))
            {
                try
                {
                    Item item = new Item();
                    item.ImportTime = DateTime.Now;

                    // required RSS elements.
                    item.Title = entry.SelectSingleNode("rss:title", xmlns).InnerText.Trim();
                    item.ContentUrl = new Uri(entry.SelectSingleNode("rss:link", xmlns).InnerText.Trim());
                    item.Description = entry.SelectSingleNode("rss:description", xmlns).InnerText.Trim();

                    // optional RDF elements.
                    // -------------------------------------------------------------------------

                    XmlNode itemPubDateNode = entry.SelectSingleNode("dc:date", xmlns);
                    if (itemPubDateNode != null && Common.IsDate(itemPubDateNode.InnerText))
                        item.PublicationTime = DateTime.Parse(itemPubDateNode.InnerText);

                    // -------------------------------------------------------------------------

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
                    this._appState.Logger.LogError("error", string.Format("RDF parsing failed for: {0}", Source.FeedUrl));
                }
            }
        }
        #endregion
    }
}