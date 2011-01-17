using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Collections.Specialized;
using System.Collections;

namespace ContinuousLinq
{
    public class ReferenceCountTracker<TSource>
    {
        private Dictionary<TSource, int> ReferenceCounts { get; set; }

        public ReferenceCountTracker()
        {
            this.ReferenceCounts = new Dictionary<TSource, int>();
        }

        public ReferenceCountTracker(IEnumerable<TSource> collection)
        {
            this.ReferenceCounts = new Dictionary<TSource, int>();

            foreach (TSource item in collection)
            {
                Add(item);
            }
        }

        public IEnumerable<TSource> Items
        {
            get { return this.ReferenceCounts.Keys; }
        }

        public int this[TSource item]
        {
            get { return this.ReferenceCounts[item]; }
        }

        /// <summary>
        /// Increments the reference count for the item.  Returns true when refrence count goes from 0 to 1.
        /// </summary>
        public bool Add(TSource item)
        {
            int currentCount;
            if (!this.ReferenceCounts.TryGetValue(item, out currentCount))
            {
                this.ReferenceCounts.Add(item, 1);
                return true;
            }

            this.ReferenceCounts[item] = currentCount + 1;
            return false;
        }

        public void Clear()
        {
            this.ReferenceCounts.Clear();
        }

        /// <summary>
        /// Decrements the reference count for the item.  Returns true when refrence count goes from 1 to 0.
        /// </summary>
        public bool Remove(TSource item)
        {
            int currentCount = this.ReferenceCounts[item];

            if (currentCount == 1)
            {
                this.ReferenceCounts.Remove(item);
                return true;
            }

            this.ReferenceCounts[item] = currentCount - 1;
            return false;
        }

        public bool Contains(TSource item)
        {
            return this.ReferenceCounts.ContainsKey(item);
        }
    }
}
