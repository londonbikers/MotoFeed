using System;
using System.Collections;

namespace MediaPanther.Framework.Caching
{
	/// <summary>
	/// Acts as a container-class for CacheManager items.
	/// </summary>
	internal class CacheItem
	{
		#region members
		private DateTime _creationDate;
		private int _requestCount;
		private object _item;
		#endregion

		#region accessors
		/// <summary>
		/// When the item was first cached, i.e. it's lifespan.
		/// </summary>
		public DateTime Created { get { return this._creationDate; } set { this._creationDate = value; } }
		/// <summary>
		/// The number of times this object has been requested from the cache.
		/// </summary>
		public int RequestCount { get { return this._requestCount; } set { this._requestCount = value; } }
		/// <summary>
		/// The actual object that is being cached.
		/// </summary>
		public object Item { get { return this._item; } set { this._item = value; } }
		#endregion

		#region constructors
		/// <summary>
		/// Createa a new CacheItem object.
		/// </summary>
		internal CacheItem()
		{
			this._requestCount = 0;
			this._creationDate = DateTime.Now;
		}
		#endregion
	}
}