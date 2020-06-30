using System;
using System.Collections;
using System.Collections.Generic;

namespace MediaPanther.Aggregator
{
    /// <summary>
    /// Provides an easy way to create custom object collections.
    /// </summary>
    public class GenericCollection<GENERICTYPE> : CollectionBase
    {
        /// <summary>
        /// Add a new item to the collection.
        /// </summary>
        /// <param name="duplicityCheck">Controls whether or not a check for duplicate entry should be performed. Will have a performance cost for larger collections.</param>
        public void Add(GENERICTYPE GenericObject, bool duplicityCheck)
        {
            bool addToCollection = true;
            if (duplicityCheck)
            {
                if (GenericObject.GetType() == typeof(Tag))
                {
                    Tag tag = GenericObject as Tag;
                    foreach (Tag collectionTag in this.InnerList)
                    {
                        if (collectionTag.Name == tag.Name)
                        {
                            addToCollection = false;
                            break;
                        }

                    }
                }
            }
            
            if (addToCollection)
                InnerList.Add(GenericObject);
        }

        /// <summary>
        /// Add a new item to the collection. Checks to see if a duplicate entry already exists. Won't add if one already exists. Use other overload to not check.
        /// </summary>
        public void Add(GENERICTYPE GenericObject)
        {
            this.Add(GenericObject, true);
        }

        public void Remove(int index)
        {
            InnerList.RemoveAt(index);
        }

        public GENERICTYPE Item(int index)
        {
            return (GENERICTYPE)InnerList[index];
        }
    }
}