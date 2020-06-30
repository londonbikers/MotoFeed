using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using MediaPanther.Aggregator.Items;

namespace MediaPanther.Aggregator.Sources
{
    public class Source : CommonBase
    {
        #region members
        private string _name;
        private Uri _homepageUrl;
        private Uri _feedUrl;
        private DateTime _lastScanTime;
        private DateTime _contentTimeStamp;
        private SourceStatus _status;
        private string _imageFilename;
        private string _description;
        private FeedFormat _feedFormat;
        private XmlDocument _latestFeedDocument;
        private GenericCollection<Item> _latestItems;
        private int _failedGetCount;
        #endregion

        #region accessors
        /// <summary>
        /// The name of the Source.
        /// </summary>
        public string Name { get { return this._name; } set { this._name = value; } }
        /// <summary>
        /// The full url of the source site homepage.
        /// </summary>
        public Uri HomepageUrl { get { return this._homepageUrl; } set { this._homepageUrl = value; } }
        /// <summary>
        /// The full url of the source xml feed.
        /// </summary>
        public Uri FeedUrl { get { return this._feedUrl; } set { this._feedUrl = value; } }
        /// <summary>
        /// The last time the source was scanned by Reaper.
        /// </summary>
        public DateTime LastScanTime { get { return this._lastScanTime; } set { this._lastScanTime = value; } }
        /// <summary>
        /// The last time the content was updated at the source.
        /// </summary>
        public DateTime ContentTimeStamp { get { return this._contentTimeStamp; } set { this._contentTimeStamp = value; } }
        /// <summary>
        /// The status of the Source.
        /// </summary>
        public SourceStatus Status { get { return this._status; } set { this._status = value; } }
        /// <summary>
        /// If present, the name of the source representative image in our media-store.
        /// </summary>
        public string ImageFilename { get { return this._imageFilename; } set { this._imageFilename = value; } }
        /// <summary>
        /// The source's description.
        /// </summary>
        public string Description { get { return this._description; } set { this._description = value; } }
        /// <summary>
        /// Indicates what format the feed is in. Not persisted.
        /// </summary>
        public FeedFormat FeedFormat { get { return this._feedFormat; } set { this._feedFormat = value; } }
        /// <summary>
        /// If present, the latest content feed from the source URL is here. Not persisted.
        /// </summary>
        public XmlDocument LatestFeedDocument { get { return this._latestFeedDocument; } set { this._latestFeedDocument = value; } }
        /// <summary>
        /// If present, the latest Items from the feed document are here. Not Persisted.
        /// </summary>
        public GenericCollection<Item> LatestItems 
        { 
            get 
            {
                if (this._latestItems == null)
                    this._latestItems = new GenericCollection<Item>();

                return this._latestItems; 
            } 
        }
        /// <summary>
        /// The number of times the source has failed a get request for feed content.
        /// </summary>
        public int FailedGetCount { get { return this._failedGetCount; } set { this._failedGetCount = value; } }
        #endregion

        #region constructors
        /// <summary>
        /// Returns a new Source object.
        /// </summary>
        public Source() 
        {
            base.ConsumerType = this.GetType();
        }
        #endregion

        #region internal methods
        /// <summary>
        /// Validates the state of the Source object.
        /// </summary>
        internal override bool IsValid()
        {
            if (this.Name == String.Empty || this.FeedUrl == null || this.HomepageUrl == null)
                return false;
            else
                return true;
        }
        #endregion
    }
}