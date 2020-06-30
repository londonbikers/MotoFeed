using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using MediaPanther.Aggregator;
using BitFactory.Logging;

namespace MediaPanther.Aggregator.Reaper
{
    /// <summary>
    /// Acts as a go-between to store application state, configuration and application global objects.
    /// </summary>
    class AppState
    {
        #region members
        private Config _config;
        private CompositeLogger _logger;
        #endregion

        #region accessors
        /// <summary>
        /// Provides access to application confiration data.
        /// </summary>
        public Config Config { get { return this._config; } }
        /// <summary>
        /// Provides access to the application logging mechanism.
        /// </summary>
        public CompositeLogger Logger { get { return this._logger; } }
        #endregion

        #region constructors
        /// <summary>
        /// Creates a new AppState object.
        /// </summary>
        public AppState()
        {
            this._config = new Config();
            this.InitialiseLogger();
        }
        #endregion

        #region public methods
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
        /// Sets up the log file paths and logging objects.
        /// </summary>
        private void InitialiseLogger()
        {
            // BitFactory Logging Setup.
            Logger errorLogger = new FileLogger(Properties.Settings.Default.LogPath + "reaper_errors.log");
            Logger debugLogger = new FileLogger(Properties.Settings.Default.LogPath + "reaper_debug.log");
            Logger infoLogger = new FileLogger(Properties.Settings.Default.LogPath + "reaper_info.log");

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
