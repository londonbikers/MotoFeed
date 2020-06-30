using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using MediaPanther.Framework;
using MediaPanther.Framework.Data;
using MediaPanther.Framework.Caching;

namespace MediaPanther.Aggregator.Categories
{
	public class CategoryServer
    {
        #region constructors
        /// <summary>
        /// Returns a new instance of the Category Server.
        /// </summary>
        internal CategoryServer()
        {
        }
        #endregion

        #region public methods
        /// <summary>
		/// Returns a collection of all the categories in the system.
		/// </summary>
		public GenericCollection<Category> GetAllCategories()
		{
			GenericCollection<Category> categories = CacheManager.RetrieveItem("AO_GetAllCategories_Output") as GenericCollection<Category>;
			if (categories == null)
			{
				int counter = 0;
				Category category;
				categories = new GenericCollection<Category>();
				SqlDataReader reader = null;
				SqlConnection connection = Server.Instance.GetDatabaseConnection();
				SqlCommand command = connection.CreateCommand();
				command.CommandText = "AO_GetAllCategories";
				command.CommandType = CommandType.StoredProcedure;

				try
				{
					reader = command.ExecuteReader();
					while (reader.Read())
					{
                        category = this.InitialiseCategory(reader);
						categories.Add(category);
						counter++;
					}

					CacheManager.AddItem(categories, "AO_GetAllCategories_Output");

					if (Server.Instance.InDebugMode)
						Server.Instance.Logger.LogDebug("debug", string.Format("{0} categories loaded from the database.", counter));
				}
				catch (Exception ex)
				{
					Server.Instance.Logger.LogError("error", string.Format("Error loading categories from database: {0}", ex.Message));
				}
				finally
				{
					if (reader != null)
						reader.Close();

					if (connection != null)
						connection.Close();
				}
			}

			return categories;
		}

		/// <summary>
		/// Retrieves a specific category.
		/// </summary>
		public Category GetCategory(int categoryID)
		{
			if (categoryID < 1)
				throw new ArgumentOutOfRangeException("categoryID");

			string appID = Category.GetApplicationUniqueID(typeof(Category), categoryID);
			Category category = CacheManager.RetrieveItem(appID) as Category;

			if (category == null)
			{
				SqlDataReader reader = null;
				SqlConnection connection = Server.Instance.GetDatabaseConnection();
				SqlCommand command = connection.CreateCommand();
				command.CommandText = "AO_GetCategory";
				command.CommandType = CommandType.StoredProcedure;
				command.Parameters.Add(new SqlParameter("ID", categoryID));

				try
				{
					reader = command.ExecuteReader();
					if (reader.Read())
                        category = this.InitialiseCategory(reader);

					CacheManager.AddItem(category, appID);
				}
				catch (Exception ex)
				{
					Server.Instance.Logger.LogError("error", string.Format("Error loading category from database:\n{0}", ex.Message));
					
					#if DEBUG
					throw;
					#endif
				}
				finally
				{
					if (reader != null)
						reader.Close();

					if (connection != null)
						connection.Close();
				}
			}

			return category;
		}
		#endregion

        #region private methods
        /// <summary>
        /// Using a fully-populated and record-advanced data-reader, a new Category object is built.
        /// </summary>
        private Category InitialiseCategory(SqlDataReader reader)
        {
            Category category = new Category();
            category.ID = (int)Data.GetValue(typeof(int), reader["ID"]);
            category.Name = Data.GetValue(typeof(string), reader["Name"]) as string;
            category.Status = (CategoryStatus)Enum.Parse(typeof(CategoryStatus), ((byte)reader["Status"]).ToString());
            category.ParentCategoryID = (int)Data.GetValue(typeof(int), reader["ParentCategoryID"]);
            category.IsPersisted = true;

            ArrayList synonums = Common.CsvToArray(Data.GetValue(typeof(string), reader["Synonyms"]) as string, ",");
            if (synonums != null)
            {
                foreach (string synonym in synonums)
                    category.Synonyms.Add(synonym.ToLower());
            }

            ArrayList tags = Common.CsvToArray(Data.GetValue(typeof(string), reader["Tags"]) as string, ",");
            if (tags != null)
            {
                foreach (string tag in tags)
                    category.Tags.Add(new Tag(tag));
            }

            return category;
        }
        #endregion
    }
}