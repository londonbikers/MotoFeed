using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MediaPanther.Aggregator.Sources;

namespace MediaPanther.Aggregator.Items
{
    public class Item : CommonBase
    {
        #region members
        private long _id;
        private Source _source;
        private DateTime _importTime;
        private DateTime _publicationTime;
        private string _title;
        private string _description;
        private Uri _contentUrl;
        private Categories.Category _category;
        private GenericCollection<Tag> _tags;
        private string _imageFilename;
        private long _views;
        private FeedData _feedData;
        #endregion

        #region accessors
        /// <summary>
        /// The Source to which this Item came from.
        /// </summary>
        public Source Source { get { return this._source; } set { this._source = value; } }
        /// <summary>
        /// The time this item was imported into the system.
        /// </summary>
        public DateTime ImportTime { get { return this._importTime; } set { this._importTime = value; } }
        /// <summary>
        /// Where possible, the time this Item was published by the source. If not available, this will be the import time.
        /// </summary>
        public DateTime PublicationTime { get { return this._publicationTime; } set { this._publicationTime = value; } }
        /// <summary>
        /// The headline for this Item.
        /// </summary>
        public string Title { get { return this._title; } set { this._title = value; } }
        /// <summary>
        /// The content summary for this Item.
        /// </summary>
        public string Description { get { return this._description; } set { this._description = value; } }
        /// <summary>
        /// The location where the full Item content can be seen.
        /// </summary>
        public Uri ContentUrl { get { return this._contentUrl; } set { this._contentUrl = value; } }
        /// <summary>
        /// The Category to which this Item belongs to.
        /// </summary>
        public Categories.Category Category { get { return this._category; } set { this._category = value; } }
        /// <summary>
        /// The individual Tags that this Item has associated with it.
        /// </summary>
        public GenericCollection<Tag> Tags 
        { 
            get 
            {
                if (this._tags == null)
                    this._tags = new GenericCollection<Tag>();

                return this._tags; 
            } 
        }
        /// <summary>
        /// If present, the filename of the cover image associated with this Item.
        /// </summary>
        public string ImageFilename { get { return this._imageFilename; } set { this._imageFilename = value; } }
        /// <summary>
        /// How many times has this Item been viewed by a client.
        /// </summary>
        public long Views { get { return this._views; } set { this._views = value; } }
        /// <summary>
        /// Provides access to feed specific data for this item. Not persisted.
        /// </summary>
        public FeedData FeedSpecificData { get { return this._feedData; } }
        #endregion

        #region constructors
        /// <summary>
        /// Instantiates a new Item object.
        /// </summary>
        public Item()
        {
            base.ConsumerType = this.GetType();
            this._feedData = new FeedData();
            this._imageFilename = String.Empty;
        }
        #endregion

        /// <summary>
        /// Contains data that relates to the original source feed, used for item processing.
        /// </summary>
        public class FeedData
        {
            #region members
            private GenericCollection<Uri> _imageUrls;
            private GenericCollection<string> _categories;
            private string _guid;
            private bool _isNew;
            #endregion

            #region accessors
            /// <summary>
            /// Xml feeds can contain category names to help identify the context of the item.
            /// </summary>
            public GenericCollection<string> Categories
            {
                get
                {
                    if (this._categories == null)
                        this._categories = new GenericCollection<string>();

                    return this._categories;
                }
                set { this._categories = value; } 
            }
            /// <summary>
            /// A unique identifier for the item. Typically it's a Uri, but it could be anything to signify uniqueness.
            /// </summary>
            public string Guid { get { return this._guid; } set { this._guid = value; } }
            /// <summary>
            /// A reference to an image relating to this Item.
            /// </summary>
            public GenericCollection<Uri> ImageUrls 
            { 
                get 
                {
                    if (this._imageUrls == null)
                        this._imageUrls = new GenericCollection<Uri>();

                    return this._imageUrls; 
                } 
                set { this._imageUrls = value; } }
            /// <summary>
            /// Indicates whether or not this item is new to us or not.
            /// </summary>
            public bool IsNewToUs { get { return this._isNew; } set { this._isNew = value; } }
            #endregion
        }
    }
}