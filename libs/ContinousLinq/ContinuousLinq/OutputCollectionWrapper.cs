using System.Collections.Generic;

namespace ContinuousLinq
{
    /// <summary>
    /// Wraps a ReadOnlyContinuousCollection so CLINQ can update it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class OutputCollectionWrapper<T>
    {
        private readonly ReadOnlyContinuousCollection<T> _inner;

        public OutputCollectionWrapper(ReadOnlyContinuousCollection<T> inner)
        {
            _inner = inner;
        }

        public void Insert(int index, T item)
        {
            try
            {
                _inner.IsSealed = false;
                _inner.Insert(index, item);
            }
            finally
            {
                _inner.IsSealed = true;
            }
        }

        public void RemoveAt(int index)
        {
            try
            {
                _inner.IsSealed = false;
                _inner.RemoveAt(index);
            }
            finally
            {
                _inner.IsSealed = true;
            }
        }

        public T this[int index]
        {
            get { return _inner[index]; }
            set
            {
                try
                {
                    _inner.IsSealed = false;
                    _inner[index] = value;
                }
                finally
                {
                    _inner.IsSealed = true;
                }
            }
        }

        public void Add(T item)
        {
            try
            {
                _inner.IsSealed = false;
                _inner.Add(item);
            }
            finally
            {
                _inner.IsSealed = true;
            }
        }

        public void Clear()
        {
            try
            {
                _inner.IsSealed = false;
                _inner.Clear();
            }
            finally
            {
                _inner.IsSealed = true;
            }
        }

        public bool Contains(T item)
        {
            return _inner.Contains(item);
        }

        public bool Remove(T item)
        {
            try
            {
                _inner.IsSealed = false;
                return _inner.Remove(item);
            }
            finally
            {
                _inner.IsSealed = true;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        #region ContinuousCollection<T> Members

        public int BinarySearch(T item, IComparer<T> comparer)
        {
            return _inner.BinarySearch(item, comparer);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            try
            {
                _inner.IsSealed = false;
                _inner.AddRange(collection);
            }
            finally
            {
                _inner.IsSealed = true;
            }
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            try
            {
                _inner.IsSealed = false;
                _inner.InsertRange(index, collection);
            }
            finally
            {
                _inner.IsSealed = true;
            }
        }

        public void RemoveRange(int index, int count)
        {
            try
            {
                _inner.IsSealed = false;
                _inner.RemoveRange(index, count);
            }
            finally
            {
                _inner.IsSealed = true;
            }
        }

        public void ReplaceRange(int index, IEnumerable<T> collection)
        {
            try
            {
                _inner.IsSealed = false;
                _inner.ReplaceRange(index, collection);
            }
            finally
            {
                _inner.IsSealed = true;
            }
        }

        #endregion
    }
}
