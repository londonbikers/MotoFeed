using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPanther.Aggregator
{
    public class Tag
    {
        #region members
        private string _name;
        #endregion

        #region accessors
        /// <summary>
        /// The full name for the tag.
        /// </summary>
        public string Name { get { return this._name; } set { this._name = value; } }
        #endregion

        #region constructors
        /// <summary>
        /// Instantiates a new Tag object.
        /// </summary>
        public Tag(string tag)
        {
            this.Name = tag;
        }
        #endregion
    }
}
