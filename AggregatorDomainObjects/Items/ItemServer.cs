using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using MediaPanther.Framework;
using MediaPanther.Framework.Caching;

namespace MediaPanther.Aggregator.Items
{
    public class ItemServer
    {
        #region constructors
        /// <summary>
        /// Returns a new instance of the Items Server.
        /// </summary>
        internal ItemServer()
        {
        }
        #endregion

        #region public methods
        /// <summary>
        /// Persists any changes to an Item object.
        /// </summary>
        public void UpdateItem(Item item)
        {
            if (item == null)
                throw new ArgumentException("Item is null!");

            SqlConnection connection = Server.Instance.GetDatabaseConnection();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;

            // normally this could be handled by a single update/create sproc, but for performance considerations
            // we'll break this out into two sprocs to minimise query overhead.
            if (item.IsPersisted)
            {
                command.CommandText = "AO_UpdateItem";
                command.Parameters.Add(new SqlParameter("@ID", item.ID));
            }
            else
            {
                command.CommandText = "AO_InsertItem";
            }

            command.Parameters.Add(new SqlParameter("@SourceID", item.Source.ID));
            command.Parameters.Add(new SqlParameter("@ImportTime", item.ImportTime));
            command.Parameters.Add(new SqlParameter("@PublicationTime", item.PublicationTime));
            command.Parameters.Add(new SqlParameter("@Title", item.Title));
            command.Parameters.Add(new SqlParameter("@Description", item.Description));
            command.Parameters.Add(new SqlParameter("@ContentUrl", item.ContentUrl.AbsoluteUri));
            command.Parameters.Add(new SqlParameter("@CategoryID", item.Category.ID));
            command.Parameters.Add(new SqlParameter("@ImageFilename", item.ImageFilename));

            StringBuilder tagsCSV = new StringBuilder();
            for (int i = 0; i < item.Tags.Count; i++)
            {
                tagsCSV.Append(item.Tags.Item(i).Name);
                if (i < (item.Tags.Count - 1))
                    tagsCSV.Append(", ");
            }

            command.Parameters.Add(new SqlParameter("@Tags", tagsCSV.ToString()));

            try
            {
                if (item.IsPersisted)
                {
                    command.ExecuteNonQuery();
                }
                else
                {
                    item.ID = long.Parse(command.ExecuteScalar().ToString());
                    CacheManager.AddItem(item, item.ApplicationUniqueID);
                    item.IsPersisted = true;
                }
            }
            catch (Exception ex)
            {
                Server.Instance.Logger.LogError("error", string.Format("Error persisting item to database: {0}", ex.Message));

                #if DEBUG
                throw;
                #endif
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }
        #endregion
    }
}