using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Configuration;
using MediaPanther.Aggregator.Categories;
using MediaPanther.Aggregator.Items;
using MediaPanther.Aggregator.Sources;
using MediaPanther.Framework;
using MediaPanther.Framework.Caching;
using BitFactory.Logging;

namespace MediaPanther.Aggregator
{
    /// <summary>
    /// Provides retrieval and persistence services for the business objects to application clients.
    /// </summary>
    public class Server
    {
        #region members
		private static Server _server = new Server();
		private CompositeLogger _logger;
		private CategoryServer _categoryServer;
        private ItemServer _itemServer;
        private SourceServer _sourcerServer;
        private bool _inDebugMode;
        #endregion

        #region public accessors
		/// <summary>
		/// Retrieves the single instance of the application.
		/// </summary>
		public static Server Instance { get { return _server; } }
		/// <summary>
		/// Provides access to category-based functionality.
		/// </summary>
		public CategoryServer CategoryServer { get { return this._categoryServer; } }
        /// <summary>
        /// Provides access to item-based functionality.
        /// </summary>
        public ItemServer ItemServer { get { return this._itemServer; } }
        /// <summary>
        /// Provides access to sources-based functionality.
        /// </summary>
        public SourceServer SourceServer { get { return this._sourcerServer; } }
        #endregion

        #region internal accessors
        /// <summary>
        /// Provides access to the applications logging mechanism.
        /// </summary>
        internal CompositeLogger Logger { get { return this._logger; } }
        /// <summary>
        /// Highlights whether or not the application is running in debug mode.
        /// </summary>
        internal bool InDebugMode { get { return this._inDebugMode; } }
        #endregion

        #region constructors
        /// <summary>
        /// Returns a new Server instance.
        /// </summary>
        protected Server()
        {
			// basic init.
			this.InitialiseLogger();

			// server members init.
			this._categoryServer = new CategoryServer();
            this._itemServer = new ItemServer();
            this._sourcerServer = new SourceServer();
            
            // config values.
            this._inDebugMode = Properties.Settings.Default.InDebugMode;
        }
        #endregion

        #region internal methods
        /// <summary>
        /// Provides a new and open connection to the application database. Close when finished.
        /// </summary>
        internal SqlConnection GetDatabaseConnection()
        {
            SqlConnection connection = new SqlConnection(Properties.Settings.Default.ConnectionString);
            connection.Open();
            return connection;
        }
        #endregion

        #region private methods
        /// <summary>
		/// Initialises the application logging feature.
		/// </summary>
		private void InitialiseLogger()
		{
			Logger errorLogger = new FileLogger(Properties.Settings.Default.LogPath + "ao_error.log");
            Logger debugLogger = new FileLogger(Properties.Settings.Default.LogPath + "ao_debug.log");
            Logger infoLogger = new FileLogger(Properties.Settings.Default.LogPath + "ao_info.log");

			LogEntryCategoryFilter errorFilter = new LogEntryCategoryFilter(true);
			errorFilter.AddCategory("error");
			LogEntryCategoryFilter debugFilter = new LogEntryCategoryFilter(true);
			debugFilter.AddCategory("debug");
			LogEntryCategoryFilter infoFilter = new LogEntryCategoryFilter(true);
			infoFilter.AddCategory("info");

			errorLogger.Filter = errorFilter;
			debugLogger.Filter = debugFilter;
			infoLogger.Filter = infoFilter;

			this._logger = new CompositeLogger();
			this._logger.AddLogger("Error Log", errorLogger);
			this._logger.AddLogger("Debug Log", debugLogger);
			this._logger.AddLogger("Info Log", infoLogger);
		}
		#endregion
	}
}
