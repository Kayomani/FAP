/*
 * Based on code from
 *  http://blog.mridey.com/2009/05/filtered-observablecollection.html
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Fap.Foundation
{
    public class SafeFilteredObservingCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private SafeObservingCollection<T> _collection;
        private Predicate<T> _filter;
        private event NotifyCollectionChangedEventHandler _collectionchanged;
        private event PropertyChangedEventHandler _propertychanged;

        public SafeFilteredObservingCollection(SafeObservingCollection<T> collection)
        {
            _filter = null;
            _collection = collection;
            _collection.CollectionChanged += new NotifyCollectionChangedEventHandler(OnCollectionChanged);
            ((INotifyPropertyChanged)_collection).PropertyChanged += new PropertyChangedEventHandler(OnPropertyChanged);
        }

        public void Add(T item)
        {
            if (_filter != null && _filter(item) == false)
                throw new InvalidOperationException();
            _collection.Add(item);
        }

        public int Add(object value)
        {
            Add((T)value);
            return IndexOf(value);
        }

        public void Clear()
        {
            if (_filter == null)
                _collection.Clear();
            else
                for (int i = 0; i < _collection.Count; )
                {
                    T item = _collection[i];
                    if (_filter(item))
                        _collection.RemoveAt(i);
                    else
                        i++;
                }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { _collectionchanged += value; }
            remove { _collectionchanged -= value; }
        }

        public bool Contains(T item)
        {
            if (_filter != null && _filter(item) == false)
                return false;
            return _collection.Contains(item);
        }

        public bool Contains(object value)
        {
            if (_filter != null && _filter((T)value) == false)
                return false;
            return ((IList)_collection).Contains(value);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (_filter == null)
                _collection.CopyTo(array, arrayIndex);
            else
            {
                for (int i = 0; i < _collection.Count; i++)
                {
                    T item = _collection[i];
                    if (_filter(item))
                        array[arrayIndex++] = item;
                }
            }
        }

        public void CopyTo(Array array, int index)
        {
            if (_filter == null)
                ((IList)_collection).CopyTo(array, index);
            else
            {
                for (int i = 0; i < _collection.Count; i++)
                {
                    T item = _collection[i];
                    if (_filter(item))
                        array.SetValue(item, index++);
                }
            }
        }

        public int Count
        {
            get
            {
                try
                {
                    _collection.Lock();
                    if (_filter == null)
                        return _collection.Count;
                    else
                    {
                        int count = 0;
                        for (int i = 0; i < _collection.Count; i++)
                        {
                            T item = _collection[i];
                            if (_filter(item))
                                count++;
                        }
                        return count;
                    }
                }
                finally
                {
                    _collection.Unlock();
                }
            }
        }

        public Predicate<T> Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new SafeFilteredObservingCollectionEnumerator(this, _collection.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SafeFilteredObservingCollectionEnumerator(this, _collection.GetEnumerator());
        }

        public int IndexOf(T item)
        {
            if (_filter == null)
                return _collection.IndexOf(item);
            else
            {
                int count = 0;
                for (int i = 0; i < _collection.Count; i++)
                {
                    T indexitem = _collection[i];
                    if (_filter(indexitem))
                    {
                        if (indexitem.Equals(item))
                            return count;
                        else
                            count++;
                    }
                }
                return -1;
            }
        }

        public int IndexOf(object value)
        {
            if (value is T)
                return IndexOf((T)value);
            else
                return -1;
        }

        public void Insert(int index, T item)
        {
            if (_filter != null && _filter(item) == false)
                throw new InvalidOperationException();

            if (_filter == null || index == 0)
                _collection.Insert(index, item);
            else
            {
                for (int i = 0; i < _collection.Count; i++)
                {
                    T indexitem = _collection[i];
                    if (_filter(indexitem))
                    {
                        index--;
                        if (index == 0)
                        {
                            _collection.Insert(i + 1, item);
                            return;
                        }
                    }
                }
                throw new ArgumentOutOfRangeException();
            }
        }

        public void Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        public bool IsFixedSize
        {
            get { return ((IList)_collection).IsFixedSize; }
        }

        public bool IsReadOnly
        {
            get { return ((IList)_collection).IsReadOnly; }
        }

        public bool IsSynchronized
        {
            get { return ((IList)_collection).IsSynchronized; }
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { _propertychanged += value; }
            remove { _propertychanged -= value; }
        }

        public bool Remove(T item)
        {
            if (_filter != null && _filter(item) == false)
                return false;

            return _collection.Remove(item);
        }

        public void Remove(object value)
        {
            if (!(value is T) || (_filter != null && _filter((T)value) == false))
                return;

            ((IList)_collection).Remove(value);
        }

        public void RemoveAt(int index)
        {
            if (_filter == null)
                _collection.RemoveAt(index);
            else
            {
                for (int i = 0; i < _collection.Count; i++)
                {
                    T indexitem = _collection[i];
                    if (_filter(indexitem))
                    {
                        if (index == 0)
                        {
                            _collection.RemoveAt(i);
                            return;
                        }
                        else
                            index--;
                    }
                }
                throw new ArgumentOutOfRangeException();
            }
        }

        public object SyncRoot
        {
            get { return ((IList)_collection).SyncRoot; }
        }

        public T this[int index]
        {
            get
            {
                if (_filter == null)
                    return _collection[index];
                else
                {
                    for (int i = 0; i < _collection.Count; i++)
                    {
                        T indexitem = _collection[i];
                        if (_filter(indexitem))
                        {
                            if (index == 0)
                            {
                                return indexitem;
                            }
                            else
                                index--;
                        }
                    }
                    throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                if (_filter == null)
                    _collection[index] = value;
                else if (_filter(value) == false)
                    throw new InvalidOperationException();
                else
                {
                    for (int i = 0; i < _collection.Count; i++)
                    {
                        T indexitem = _collection[i];
                        if (_filter(indexitem))
                        {
                            if (index == 0)
                            {
                                _collection[i] = value;
                            }
                            else
                                index--;
                        }
                    }
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        object IList.this[int index]
        {
            get
            {
                return (object)this[index];
            }
            set
            {
                this[index] = (T)value;
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_collectionchanged != null)
            {
                // Check the NewItems
                List<T> newlist = new List<T>();
                if (e.NewItems != null)
                    foreach (T item in e.NewItems)
                        if (_filter(item) == true)
                            newlist.Add(item);

                // Check the OldItems
                List<T> oldlist = new List<T>();
                if (e.OldItems != null)
                    foreach (T item in e.OldItems)
                        if (_filter(item) == true)
                            oldlist.Add(item);

                // Create the Add/Remove/Replace lists
                List<T> addlist = new List<T>();
                List<T> removelist = new List<T>();
                List<T> replacelist = new List<T>();

                // Fill the Add/Remove/Replace lists
                foreach (T item in newlist)
                    if (oldlist.Contains(item))
                        replacelist.Add(item);
                    else
                        addlist.Add(item);
                foreach (T item in oldlist)
                    if (newlist.Contains(item))
                        continue;
                    else
                        removelist.Add(item);

                // Send the corrected event
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    case NotifyCollectionChangedAction.Move:
                    case NotifyCollectionChangedAction.Remove:
                    case NotifyCollectionChangedAction.Replace:
                        if (addlist.Count > 0)
                            _collectionchanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, addlist));
                        if (replacelist.Count > 0)
                            _collectionchanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, replacelist));
                        if (removelist.Count > 0)
                            _collectionchanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removelist));
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        _collectionchanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                        break;
                }
            }
        }

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_propertychanged != null)
                _propertychanged(this, e);
        }

        private class SafeFilteredObservingCollectionEnumerator : IEnumerator<T>, IEnumerator
        {
            private SafeFilteredObservingCollection<T> _filteredcollection;
            private IEnumerator<T> _enumerator;

            public SafeFilteredObservingCollectionEnumerator(SafeFilteredObservingCollection<T> filteredcollection, IEnumerator<T> enumerator)
            {
                _filteredcollection = filteredcollection;
                _enumerator = enumerator;
            }

            public T Current
            {
                get
                {
                    if (_filteredcollection.Filter == null)
                        return _enumerator.Current;
                    else if (_filteredcollection.Filter(_enumerator.Current) == false)
                        throw new InvalidOperationException();
                    else
                        return _enumerator.Current;
                }
            }

            public void Dispose()
            {
                _enumerator.Dispose();
            }

            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            public bool MoveNext()
            {
                while (true)
                {
                    if (_enumerator.MoveNext() == false)
                        return false;
                    if (_filteredcollection.Filter == null
                        || _filteredcollection.Filter(_enumerator.Current) == true)
                        return true;
                }
            }

            public void Reset()
            {
                _enumerator.Reset();
            }
        }
    }
}

