using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using MediaPanther.Framework;
using MediaPanther.Framework.Data;
using MediaPanther.Framework.Caching;

namespace MediaPanther.Aggregator.Sources
{
    public class SourceServer
    {
        #region constructors
        /// <summary>
        /// Returns a new instance of the Sources Server.
        /// </summary>
        internal SourceServer()
        {
        }
        #endregion

        #region public methods
        /// <summary>
        /// Persists any changes to a new or updated Source object.
        /// </summary>
        public void UpdateSource(Source source)
        {
            if (source == null || !source.IsValid())
                throw new ArgumentException("Source is null or invalid!");

            SqlConnection connection = Server.Instance.GetDatabaseConnection();
            SqlCommand command = connection.CreateCommand();
            command.CommandText = "AO_UpdateSource";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("ID", source.ID));
            command.Parameters.Add(new SqlParameter("Name", source.Name));
            command.Parameters.Add(new SqlParameter("HomepageUrl", source.HomepageUrl.AbsoluteUri));
            command.Parameters.Add(new SqlParameter("FeedUrl", source.FeedUrl.AbsoluteUri));
            command.Parameters.Add(new SqlParameter("LastScanTime", source.LastScanTime));
            command.Parameters.Add(new SqlParameter("ContentTimeStamp", source.ContentTimeStamp));
            command.Parameters.Add(new SqlParameter("Status", (int)source.Status));
            command.Parameters.Add(new SqlParameter("ImageFilename", source.ImageFilename));
            command.Parameters.Add(new SqlParameter("Description", source.Description));
            command.Parameters.Add(new SqlParameter("FailedGetCount", source.FailedGetCount));

            try
            {
                source.ID = Convert.ToInt32(command.ExecuteScalar());

                // if new, add to the cache.
                if (!source.IsPersisted)
                {
                    CacheManager.AddItem(source, source.ApplicationUniqueID);
                    source.IsPersisted = true;
                }
            }
            catch (Exception ex)
            {
                Server.Instance.Logger.LogError("ERROR", string.Format("Unable to update Source: {0}\n{1}", ex.Message, ex.StackTrace));
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }

        /// <summary>
        /// Constructs a new Source object from a datareader.
        /// </summary>
        /// <remarks>
        /// Hrm, needs to be refactored, don't want to expose DAL stuff.
        /// </remarks>
        public Source InitialiseSourceFromReader(SqlDataReader reader)
        {
            Source source = new Source();
            source.IsPersisted = true;

            source.ID = (int)reader["ID"];
            source.Name = Data.GetValue(typeof(string), reader["Name"]) as string;
            source.HomepageUrl = new Uri(Data.GetValue(typeof(string), reader["HomepageUrl"]) as string);
            source.FeedUrl = new Uri(Data.GetValue(typeof(string), reader["FeedUrl"]) as string);
            source.ContentTimeStamp = (DateTime)Data.GetValue(typeof(DateTime), reader["ContentTimeStamp"]);
            source.Status = (SourceStatus)int.Parse(reader["Status"].ToString());
            source.ImageFilename = Data.GetValue(typeof(string), reader["ImageFilename"]) as string;
            source.Description = Data.GetValue(typeof(string), reader["Description"]) as string;
            source.FailedGetCount = (byte)Data.GetValue(typeof(byte), reader["FailedGetCount"]);

            return source;
        }

        ///// <summary>
        ///// Streams a live feed of Sources straight from the database. Used to handle very large data-sets.
        ///// </summary>
        ///// <param name="status"></param>
        ///// <returns></returns>
        //public SourceStreamer StreamAllSources(SourceStatus status)
        //{
        //}
        #endregion
    }
}