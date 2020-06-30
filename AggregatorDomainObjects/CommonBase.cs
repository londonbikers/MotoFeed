using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPanther.Aggregator
{
    /// <summary>
    /// Base class offering common functionality to all application domain objects.
    /// </summary>
	public class CommonBase
	{
		#region members
		private long _id;
		private bool _isPersisted;
		private Type _consumerType;
		#endregion

		#region accessors
		/// <summary>
		/// The identifier for the consuming object.
		/// </summary>
		public long ID { get { return this._id; } set { this._id = value; } }
		/// <summary>
		/// Determines if the consuming object requires persistance or not.
		/// </summary>
		internal bool IsPersisted { get { return this._isPersisted; } set { this._isPersisted = value; } }
		/// <summary>
		/// Generates a unique ID for the consuming object within the context of the application. Used for objects that implement numeric ID's.
		/// </summary>
		internal string ApplicationUniqueID { get { return CommonBase.GetApplicationUniqueID(this._consumerType, this.ID); } }
        /// <summary>
        /// REQUIRED! The type of the derived object for this base class.
        /// </summary>
        internal Type ConsumerType { get { return this._consumerType; } set { this._consumerType = value; } }
		#endregion

		#region constructors
		internal CommonBase()
		{
			this._isPersisted = false;
		}
		#endregion

        #region internal methods
        /// <summary>
        /// Validates the object.
        /// </summary>
        internal virtual bool IsValid()
        {
            return true;
        }
        #endregion

        #region static methods
        /// <summary>
		/// Retrieves an application-wide unique identifier for a domain object type.
		/// </summary>
		internal static string GetApplicationUniqueID(Type consumerType, long id)
		{
			return consumerType.FullName + ":" + id.ToString();
		}
		#endregion
	}
}