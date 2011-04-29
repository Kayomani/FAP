using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Threading;
using System.Collections;
using System.Collections.ObjectModel;

namespace Fap.Foundation
{
    /// <summary>
    /// A thread safe collection designed to be updated primarily by background threads
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SafeObservedCollection<T> : IList<T>, ICollection<T>, INotifyCollectionChanged
    {
        private IList<T> collection = new List<T>();
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event NotifyCollectionChangedEventHandler OnCollectionChanged;
        private ReaderWriterLock sync = new ReaderWriterLock();

        public void AddRotate(T item, int max)
        {
            sync.AcquireWriterLock(Timeout.Infinite);
            Add(item);
            if (Count > max)
                RemoveAt(0);
            sync.ReleaseWriterLock();
        }

        public void Add(T item)
        {
            sync.AcquireWriterLock(Timeout.Infinite);
            collection.Add(item);

            int index = -1;

            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (collection[i].Equals(item))
                {
                    index = i;
                    break;
                }
            }

            if (null != OnCollectionChanged)
                OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            sync.ReleaseWriterLock();
            if (CollectionChanged != null)
                CollectionChanged(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public T Pop()
        {
            sync.AcquireWriterLock(Timeout.Infinite);
            T item = default(T); 
            if (collection.Count > 0)
            {
                item = collection.First();
                collection.RemoveAt(0);
                if (null != OnCollectionChanged)
                    OnCollectionChanged(this,new
                        NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, 0));
            }
            sync.ReleaseWriterLock();
            if (null != item)
            {
                if (CollectionChanged != null)
                    CollectionChanged(this, new
                        NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, 0));
            }
            return item;
        }

        public void Lock()
        {
            sync.AcquireWriterLock(Timeout.Infinite);
        }

        public void Unlock()
        {
            sync.ReleaseWriterLock();
        }

        public List<T> ToList()
        {
            sync.AcquireWriterLock(Timeout.Infinite);
            var list = collection.ToList();
            sync.ReleaseWriterLock();
            return list;
        }

        public void Clear()
        {
            sync.AcquireWriterLock(Timeout.Infinite);
            collection.Clear();
            if (null != OnCollectionChanged)
                OnCollectionChanged(this,new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            sync.ReleaseWriterLock();
            if (CollectionChanged != null)
                CollectionChanged(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(T item)
        {
            sync.AcquireReaderLock(Timeout.Infinite);
            var result = collection.Contains(item);
            sync.ReleaseReaderLock();
            return result;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            sync.AcquireWriterLock(Timeout.Infinite);
            collection.CopyTo(array, arrayIndex);
            sync.ReleaseWriterLock();
        }

        public int Count
        {
            get
            {
                sync.AcquireReaderLock(Timeout.Infinite);
                var result = collection.Count;
                sync.ReleaseReaderLock();
                return result;
            }
        }

        public bool IsReadOnly
        {
            get { return collection.IsReadOnly; }
        }

        public bool Remove(T item)
        {
            sync.AcquireWriterLock(Timeout.Infinite);
            var index = collection.IndexOf(item);
            if (index == -1)
            {
                sync.ReleaseWriterLock();
                return false;
            }
            var result = collection.Remove(item);
            if (null != OnCollectionChanged)
                OnCollectionChanged(this,new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            sync.ReleaseWriterLock();
            if (result && CollectionChanged != null)
                CollectionChanged(this, new
                    NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            return result;
        }

        public bool Remove(IList items)
        {
            sync.AcquireWriterLock(Timeout.Infinite);
           
            foreach (var item in items)
            {
                if (item is T)
                {
                    if (collection.Contains((T)item))
                    {
                        if (null != OnCollectionChanged)
                            OnCollectionChanged(this,new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, collection.IndexOf((T)item)));
                        collection.Remove((T)item);
                    }
                }
            }
           
            sync.ReleaseWriterLock();
            CollectionChanged(this, new
                NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            sync.AcquireReaderLock(Timeout.Infinite);
            var result = collection.IndexOf(item);
            sync.ReleaseReaderLock();
            return result;
        }

        public void Insert(int index, T item)
        {
            sync.AcquireWriterLock(Timeout.Infinite);
            collection.Insert(index, item);
            if (null != OnCollectionChanged)
                OnCollectionChanged(this,new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            sync.ReleaseWriterLock();
            if (CollectionChanged != null)
                CollectionChanged(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }


        public void RemoveAt(int index)
        {
            sync.AcquireWriterLock(Timeout.Infinite);
            if (collection.Count == 0 || collection.Count <= index)
            {
                sync.ReleaseWriterLock();
                return;
            }
            T item = collection[index];
            collection.RemoveAt(index);
            if (null != OnCollectionChanged)
                OnCollectionChanged(this,new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            sync.ReleaseWriterLock();
            if (CollectionChanged != null)
                CollectionChanged(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        public T this[int index]
        {
            get
            {
                sync.AcquireReaderLock(Timeout.Infinite);
                var result = collection[index];
                sync.ReleaseReaderLock();
                return result;
            }
            set
            {
                sync.AcquireWriterLock(Timeout.Infinite);
                if (collection.Count == 0 || collection.Count <= index)
                {
                    sync.ReleaseWriterLock();
                    return;
                }
                collection[index] = value;
                if (null != OnCollectionChanged)
                    OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, index));
                sync.ReleaseWriterLock();
                if (CollectionChanged != null)
                    CollectionChanged(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, index));
            }
        }
    }
}
