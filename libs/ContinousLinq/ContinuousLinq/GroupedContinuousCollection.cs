using System;
using System.Collections.Generic;
using System.Linq;

namespace ContinuousLinq
{    
    /// <summary>
    /// This class was created to be the outbound results of a continuous query that used a group-by
    /// clause. The grouping results are stored as a "continous collection of continuous collections", where
    /// each continuous collection in the outbound collection is actually a GroupedContinuousCollection, which adds
    /// the IGrouping interface support.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    public class GroupedContinuousCollection<TKey, TElement> :
        ReadOnlyContinuousCollection<TElement>, IGrouping<TKey, TElement>,
        IEquatable<GroupedContinuousCollection<TKey, TElement>>
    {
        private readonly TKey _key;
        private readonly IEqualityComparer<TKey> _comparer;

        internal GroupedContinuousCollection(TKey key, IEqualityComparer<TKey> comparer)
        {
            _key = key;
            _comparer = comparer;
        }

        #region IGrouping<TKey,TSource> Members

        public TKey Key
        {
            get { return _key; }
        }

        #endregion

        #region IEquatable<GroupedContinuousCollection<TSource,TKey>> Members

        public bool Equals(GroupedContinuousCollection<TKey, TElement> other)
        {
            return _comparer.Equals(this.Key, other.Key);
        }

        #endregion                  
    }
}
