using System;
using System.Collections;
using System.Configuration;

namespace MediaPanther.Framework.Caching
{
	/// <summary>
	/// Provides functionality to cache domain objects in memory.
	/// </summary>
	public class CacheManager
	{
		#region members
		private static int _itemCeiling;
		private static Hashtable _cache;
		#endregion

		#region accessors
		/// <summary>
		/// Controls the maximum number of items that should be kept in the cache at any one time.
		/// </summary>
		public static int ItemCeiling { get { return _itemCeiling; } set { _itemCeiling = value; } }
		/// <summary>
		/// The number of items currently in the Cache.
		/// </summary>
		public static int ItemCount { get { return _cache.Count; } }
		#endregion

		#region constructors
		/// <summary>
		/// Creates a new CacheManager object.
		/// </summary>
		static CacheManager()
		{
			// set a default ceiling.
			_itemCeiling = Properties.Settings.Default.MaxCacheItems;
			_cache = new Hashtable();
		}
		#endregion

		#region public methods
		/// <summary>
		/// Adds a new object to the cache.
		/// </summary>
		/// <param name="item">The actual object to store in the cache.</param>
		/// <param name="key">The unique-identifier for the object.</param>
		public static void AddItem(object item, object key)
		{
			if (!_cache.ContainsKey(key))
			{
				CacheItem cacheItem = new CacheItem();
				cacheItem.Item = item;

				// is the cache full?
				if (_cache.Count == _itemCeiling)
					RemoveUnpopularItem();

				_cache.Add(key, cacheItem);
			}
			else
			{
				#if DEBUG
				throw new Exception("New Cache item already exists in the Cache!");
				#endif
			}
		}

		/// <summary>
		/// Removes an object from the cache.
		/// </summary>
		/// <param name="key">The unique-identifier for the item to be removed.</param>
		public static void RemoveItem(object key)
		{
			_cache.Remove(key);
		}

		/// <summary>
		/// Collects an object that has been cached previously. Will return null if no such item found.
		/// </summary>
		/// <param name="key">The unique-identifier for the item to be found.</param>
		public static object RetrieveItem(object key)
		{
			CacheItem item = _cache[key] as CacheItem;

			if (item != null)
			{
				item.RequestCount++;
				return item.Item;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Retrieve the date when a cached item was first cached.
		/// </summary>
		/// <param name="key">The unique-identifier for the item to be found.</param>
		/// <returns>A DateTime for a cached object, if no such cached object was found then DateTime.Min is returned.</returns>
		public static DateTime RetrieveItemCreationDate(object key)
		{
			CacheItem item = _cache[key] as CacheItem;

			if (item != null)
				return item.Created;
			else
				return DateTime.MinValue;
		}

		/// <summary>
		/// Empties the cache of all items.
		/// </summary>
		public static void FlushCache()
		{
			_cache.Clear();
		}

		/// <summary>
		/// Retrieves the top X amount of most popular CacheItems.
		/// </summary>
		/// <param name="count">The number of items to retrieve.</param>
		public static ArrayList RetrieveTopItems(int count)
		{
			if (count > _cache.Count)
				count = _cache.Count;

			ArrayList items = new ArrayList();
			CacheItem item = null;
			CacheItem topItem = null;
			int topCount;

			// manually sort the popular items out.
			for (int i = 0; i < count; i++)
			{
				topCount = -1;

				// retrieve most popular item.
				foreach (DictionaryEntry entry in _cache)
				{
					item = entry.Value as CacheItem;
					if (item.RequestCount > topCount)
					{
						// don't add any previous top items.
						if (!items.Contains(item))
						{
							topItem = item;
							topCount = item.RequestCount;
						}
					}
				}

				items.Add(topItem);
			}

			return items;
		}

		/// <summary>
		/// Returns an application-unique identifier for a particular domain object.
		/// </summary>
		/// <param name="type">The type for the domain object.</param>
		/// <param name="id">The numeric ID for the domain object.</param>
		public static string GetApplicationUniqueID(Type type, int id)
		{
			return type.FullName + ":" + id.ToString();
		}
		#endregion

		#region private methods
		/// <summary>
		/// Removes the first unpopular item from the Cache to make room.
		/// </summary>
		private static void RemoveUnpopularItem()
		{
			for (int i = 0; i < 100; i++)
			{
				IDictionaryEnumerator myEnum = _cache.GetEnumerator();
				while (myEnum.MoveNext())
				{
					if (((CacheItem)myEnum.Value).RequestCount == i)
					{
						_cache.Remove(myEnum.Key);
						return;
					}
				}
			}

			// no unpopular (less than 100 requests) items found, remove the first item instead.
			foreach (CacheItem cacheItem in _cache)
			{
				_cache.Remove(cacheItem);
				break;
			}
			return;
		}
		#endregion
	}
}