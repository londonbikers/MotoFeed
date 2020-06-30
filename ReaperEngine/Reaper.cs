using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Xml;
using System.Threading;
using System.Diagnostics;
using MediaPanther.Aggregator;
using MediaPanther.Aggregator.Sources;
using MediaPanther.Aggregator.Reaper.Exceptions;
using MediaPanther.Framework;
using BitFactory.Logging;

namespace MediaPanther.Aggregator.Reaper
{
    public class Reaper
    {
        #region members
        private DateTime _scanStartTime;
        private AppState _appState;
        private bool _run;
        private long _runs;
        #endregion

        #region accessors
        /// <summary>
        /// Application State object.
        /// </summary>
        private AppState AppState { get { return this._appState; } }
        #endregion

        #region constructors
        /// <summary>
        /// Creates a new instance of the Reaper.
        /// </summary>
        public Reaper()
        {
        }
        #endregion

        #region public methods
        /// <summary>
        /// Finishes processing after the current cycle.
        /// </summary>
        public void Stop()
        {
            this._run = false;
            AppState.Logger.LogInfo("info", string.Format("Stopping Reaper after {0} runs.", this._runs + 1));
        }

        /// <summary>
        /// Starts the service.
        /// </summary>
        public void Start()
        {
            this._run = true;
            this._runs = 0;

            this.InitialiseLogger();
            this.AppState.Logger.LogInfo("info", "Reaper Started!");

            SqlDataReader sourceReader = null;
            SqlConnection sourceConnection = AppState.GetDatabaseConnection();
            SqlCommand sourceCommand = new SqlCommand("Reaper_GetActiveSources");
            sourceCommand.Connection = sourceConnection;
            sourceCommand.CommandType = CommandType.StoredProcedure;

            while (this._run)
            {
                this._scanStartTime = DateTime.Now;

                try
                {
                    sourceReader = sourceCommand.ExecuteReader();
                    while (sourceReader.Read())
                    {
                        // TODO: Refactor using some sort of streaming source system from SourceServer.
                        Source source = Server.Instance.SourceServer.InitialiseSourceFromReader(sourceReader);

                        // we need the feed in string form first so we can then work out the format, and which
                        // handler to use for working on it.
                        this.RetrieveFeedFromUrl(source);

                        // no feed document could be retrieved, move to next source.
                        if (source.LatestFeedDocument == null)
                            continue;

                        // is this an RSS or Atom feed?
                        if (!this.DetermineFeedFormat(source))
                            continue;

                        this.ConvertFeedItemsToSourceItems(source);
                        this.ImportNewFeedItems(source);
                    }
                }
                catch (Exception ex)
                {
                    this.AppState.Logger.LogFatal("error", "Source collection: " + ex.Message + "\n" + ex.StackTrace);
                    this.AppState.Logger.LogInfo("info", "Reaper stopping.");

                    if (sourceConnection != null)
                        sourceConnection.Close();

                    throw new Exceptions.SourceScanException("Main sources scan failed.");
                }
                finally
                {
                    if (sourceReader != null)
                        sourceReader.Close();
                }

                this._runs++;
                
                if (AppState.Config.InDebugMode)
                {
                    TimeSpan runTime = DateTime.Now - _scanStartTime;
                    this.AppState.Logger.LogInfo("debug", string.Format("Scan cycle #{0} complete after {1}hrs, {2}mins, {3}secs, {4}ms.", this._runs, runTime.Hours, runTime.Minutes, runTime.Seconds, runTime.Milliseconds));
                }

                this.PauseForScanInterval();

                // DEBUG: 1 cycle.
                //this._run = false;
            }

            this.AppState.Logger.LogInfo("info", "Reaper finished processing.");
        }
        #endregion

        #region private methods
        /// <summary>
        /// Queries a remote host for an xml feed.
        /// </summary>
        /// <param name="source">The Source to retrieve the feed for.</param>
        private void RetrieveFeedFromUrl(Source source)
        {
            int requestAttempts = 0;
            XmlDocument document = new XmlDocument();

            while (requestAttempts < this.AppState.Config.SourceRetryLimit)
            {
                try
                {
                    document.Load(source.FeedUrl.AbsoluteUri);
                    source.LatestFeedDocument = document;
                    return;
                }
                catch
                {
                    // problem loading xml document. Transaction error or document malformed.
                    requestAttempts++;

                    //DEV: introduce pause before retry?
                }
            }

            // document cannot be loaded.
            this.AppState.Logger.LogWarning("info", "Couldn't load feed from: " + source.FeedUrl + ", after " + requestAttempts.ToString() + " attempts.");
            source.FailedGetCount++;

            if (source.FailedGetCount >= 3)
            {
                // disable source.
                source.Status = SourceStatus.NotResponding;
            }

            Server.Instance.SourceServer.UpdateSource(source);
        }

        /// <summary>
        /// Inspects an rss feed and determines what format it is.
        /// </summary>
        /// <param name="source">The Source containing the feed to determine the format for.</param>
        /// <returns>A boolean indicating whether or not the format could be determined.</returns>
        private bool DetermineFeedFormat(Source source)
        {
            if (source.LatestFeedDocument == null)
                return false;

            XmlNode node = source.LatestFeedDocument.DocumentElement;
            if (node.Name.ToLower() == "rdf:rdf")
            {
                // alernative name for a very basic RSS format.
                source.FeedFormat = FeedFormat.RDF;
                return true;
            }
            else if (node.Name.ToLower() == "rss")
            {
                XmlAttribute versionAttrib = node.Attributes["version"];
                if (versionAttrib != null)
                {
                    if (versionAttrib.Value == "0.90")
                        source.FeedFormat = FeedFormat.Rss0Point90;
                    else if (versionAttrib.Value == "0.91")
                        source.FeedFormat = FeedFormat.Rss0Point91;
                    else if (versionAttrib.Value == "0.92")
                        source.FeedFormat = FeedFormat.Rss0Point92;
                    else if (versionAttrib.Value == "2.0")
                        source.FeedFormat = FeedFormat.Rss2Point0;
                    else
                        throw new Exceptions.FeedFormatException(string.Format("Unrecognised RSS format for: {0}, version: {1}.", source.FeedUrl, versionAttrib.Value));

                    return true;
                }
                else
                {
                    // what's the default?
                    source.FeedFormat = FeedFormat.Rss2Point0;
                    return true;
                }
            }
            else if (node.Name.ToLower() == "feed")
            {
                XmlAttribute versionAttrib = node.Attributes["version"];
                if (versionAttrib != null)
                {
                    if (versionAttrib.Value == "0.3")
                        source.FeedFormat = FeedFormat.Atom0Point3;
                    else if (versionAttrib.Value == "1.0")
                        source.FeedFormat = FeedFormat.Atom1Point0;
                    else
                        throw new Exceptions.FeedFormatException(string.Format("Unrecognised Atom format for: {0}, version: {1}.", source.FeedUrl, versionAttrib.Value));

                    return true;
                }
                else
                {
                    source.FeedFormat = FeedFormat.Atom1Point0;
                    return true;
                }
            }

            // unhandled format detected.
            this.AppState.Logger.LogWarning("info", string.Format("Unrecognised feed format ({0}) for: {1}", node.Name, source.FeedUrl));
            return false;
        }

        /// <summary>
        /// Uses the appropriate feed handler to covert xml entries to Source Items.
        /// </summary>
        /// <param name="source">The Source object containing the feed.</param>
        private void ConvertFeedItemsToSourceItems(Source source)
        {
            if (source.LatestFeedDocument == null)
                return;

            if (source.FeedFormat == FeedFormat.Atom0Point3 || source.FeedFormat == FeedFormat.Atom1Point0)
            {
                // atom feed.
                AtomReader reader = new AtomReader(source, this.AppState);
                reader.GetFeedItems();
            }
            else if (source.FeedFormat == FeedFormat.RDF)
            {
                // RDF/RSS1.0 feed.
                RdfReader reader = new RdfReader(source, this.AppState);
                reader.GetFeedItems();
            }
            else
            {
                // rss feed.
                RssReader reader = new RssReader(source, this.AppState);
                reader.GetFeedItems();
            }
        }

        /// <summary>
        /// If new content is present in a feed, this method will import them into the content store.
        /// </summary>
        /// <param name="source">The source the feed relates to.</param>
        private void ImportNewFeedItems(Source source)
        {
            // kick this off on a new thread as processing time depends on network calls.
            Importer importer = new Importer(this.AppState, source);
            Thread importerThread = new Thread(new ThreadStart(importer.ImportNewItems));

            importerThread.IsBackground = true;
            importerThread.Start();
        }

        /// <summary>
        /// If a gap between scan cycles is defined then an appropriate pause is actioned between them.
        /// </summary>
        private void PauseForScanInterval()
        {
            // manage the execution time/scan interval.
            TimeSpan processRunTime = DateTime.Now - this._scanStartTime;
            long intervalTime = this.AppState.Config.ScanIntervalSeconds;

            if (processRunTime.TotalSeconds < intervalTime)
            {
                // we need to wait some more before starting the scan again.
                if (AppState.Config.InDebugMode)
                    this.AppState.Logger.LogDebug("debug", string.Format("Scan interval of {0} seconds started.", intervalTime));

                Thread.Sleep(TimeSpan.FromSeconds(intervalTime - processRunTime.TotalSeconds));
            }
            else if (processRunTime.TotalSeconds > intervalTime)
            {
                this.AppState.Logger.LogWarning("info", string.Format("Sources scan took longer ({0} seconds) to execute than configured interval time. Consider lengthening the interval time.", processRunTime.TotalSeconds));
            }
        }

        /// <summary>
        /// Sets all object defaults and sets up any dependencies.
        /// </summary>
        private void InitialiseLogger()
        {
            this._appState = new AppState();
        }
        #endregion
    }
}