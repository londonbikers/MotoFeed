using System;
using System.Configuration;
using System.Collections.Generic;
using System.Text;

namespace MediaPanther.Aggregator.Reaper
{
    /// <summary>
    /// Contains all data and objects necessary to run all aspects of the application.
    /// </summary>
    class Config
    {
        #region members
        private const int SOURCE_RETRY_LIMIT = 3;
        #endregion

        #region accessors
        /// <summary>
        /// The time in seconds to wait between scan cycles.
        /// </summary>
        public int ScanIntervalSeconds { get { return Properties.Settings.Default.ScanIntervalSeconds; } }
        /// <summary>
        /// The number of times to retry querying a source for a feed.
        /// </summary>
        public int SourceRetryLimit { get { return SOURCE_RETRY_LIMIT; } }
        /// <summary>
        /// The primary content-store MS SQL Server Database connection string.
        /// </summary>
		public string ConnectionString { get { return Properties.Settings.Default.ConnectionString; } }
        /// <summary>
        /// Denotes whether or not the application is running in debug mode or not. Irrespective of build mode.
        /// </summary>
        public bool InDebugMode { get { return Properties.Settings.Default.InDebugMode; } }
        #endregion

        #region constructors
        /// <summary>
        /// Instantiates a new Config object.
        /// </summary>
        public Config()
        {
        }
        #endregion
    }
}
