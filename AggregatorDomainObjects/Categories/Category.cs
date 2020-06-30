using System;
using System.Collections;
using System.Collections.Generic;

namespace MediaPanther.Aggregator.Categories
{
    public class Category : CommonBase
    {
        #region members
		private int _parentCategoryID;
        private string _name;
        private GenericCollection<string> _synonyms;
        private GenericCollection<Tag> _tags;
        private Category _parentCategory;
        private CategoryStatus _status;
        #endregion

        #region accessors
        /// <summary>
        /// The full name for this Category.
        /// </summary>
        public string Name { get { return this._name; } set { this._name = value; } }
        /// <summary>
        /// A collection of synonyms for this Category.
        /// </summary>
        public GenericCollection<string> Synonyms { get { return this._synonyms; } set { this._synonyms = value; } }
        /// <summary>
        /// The individual Tags that this Category has associated with it.
        /// </summary>
        public GenericCollection<Tag> Tags
        {
            get
            {
                if (this._tags == null)
                    this._tags = new GenericCollection<Tag>();

                return this._tags;
            }
        }
        /// <summary>
        /// Categories may optionally be nested, so if present, this provides access to the parent Category.
        /// </summary>
        public Category ParentCategory 
        { 
            get 
            {
                if (this._parentCategory == null && this.ParentCategoryID > 0)
                    this.RetrieveParentCategory();

                return this._parentCategory; 
            } 
            set { this._parentCategory = value; } 
        }
        /// <summary>
        /// The current status of the Category.
        /// </summary>
        public CategoryStatus Status { get { return this._status; } set { this._status = value; } }
		/// <summary>
		/// Used internally to seed the category with the parent category id so it can be loaded later on demand if necessary.
		/// </summary>
		internal int ParentCategoryID { get { return this._parentCategoryID; } set { this._parentCategoryID = value; } }
        #endregion

        #region constructors
        /// <summary>
        /// Instantiates a new Category object.
        /// </summary>
        public Category()
        {
            base.ConsumerType = this.GetType();
            this._synonyms = new GenericCollection<string>();
        }
        #endregion

		#region private methods
		private void RetrieveParentCategory()
		{
            this.ParentCategory = Server.Instance.CategoryServer.GetCategory(this.ParentCategoryID);
		}
		#endregion
	}
}
