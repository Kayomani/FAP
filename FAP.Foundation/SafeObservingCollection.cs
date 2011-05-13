#region Copyright Kayomani 2011.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
/**
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or any 
    later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections;
using System.Threading;
using System.ComponentModel;

namespace Fap.Foundation
{
    /// <summary>
    /// Manager class handles updating ui collections from a UI thread.
    /// </summary>
    public class SafeObservingCollectionManager
    {
        private static object sync = new object();
        private static List<ISafeObservingCollection> list = new List<ISafeObservingCollection>();
        private static bool running = false;
        private static AutoResetEvent workerEvent = new AutoResetEvent(false);

        public static void Start()
        {
            lock (sync)
            {
                if (!running)
                {
                    running = true;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(DoWork));
                }
            }
        }
        public static void UpdateNowAsync()
        {
            workerEvent.Set();
        }

        private static void DoWork(object o)
        {
            while (true)
            {
                SafeObservableStatic.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                       new Action(
                        delegate()
                        {
                            Sync();
                        }));
                workerEvent.WaitOne(333);
            }
        }

        public static void Register(ISafeObservingCollection i)
        {
            list.Add(i);
        }

        public static void Unregister(ISafeObservingCollection i)
        {
            list.Remove(i);
        }

        public static void Sync()
        {
            lock (sync)
            {
                foreach (var item in list)
                    item.DoSync();
            }
        }
    }

    public interface ISafeObservingCollection
    {
        void DoSync();
    }

    public class SafeObservingCollection<T> : IList<T>, ICollection<T>, INotifyCollectionChanged, ISafeObservingCollection, INotifyPropertyChanged, IList
    {
        private List<T> collection = new List<T>();
        private SafeObservedCollection<T> viewedCollection;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private List<NotifyCollectionChangedEventArgs> changes = new List<NotifyCollectionChangedEventArgs>();

        public SafeObservingCollection(SafeObservedCollection<T> otherCollection)
        {
            otherCollection.Lock();
            viewedCollection = otherCollection;
            otherCollection.OnCollectionChanged += new NotifyCollectionChangedEventHandler(collection_OnCollectionChanged);
            foreach (var item in otherCollection)
                collection.Add(item);
            otherCollection.Unlock();
            SafeObservingCollectionManager.Register(this);
        }

        public void AddRotate(T item, int max)
        {
            viewedCollection.AddRotate(item, max);
        }

        public void Dispose()
        {
            viewedCollection.OnCollectionChanged -= new NotifyCollectionChangedEventHandler(collection_OnCollectionChanged);
            SafeObservingCollectionManager.Unregister(this);
        }

        private void collection_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            changes.Add(e);
        }

        /// <summary>
        /// Synchronise the collections.  This should only be called from the foreground thread
        /// </summary>
        public void DoSync()
        {
            try
            {
                viewedCollection.Lock();

                if (changes.Count > 1)
                {

                }

                while (changes.Count > 0)
                {
                    var e = changes[0];
                    //Update the ui collection
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            for (int i = 0; i < e.NewItems.Count; i++)
                            {
                                this.collection.Insert(e.NewStartingIndex + i, (T)e.NewItems[i]);
                            }
                            break;

                        case NotifyCollectionChangedAction.Move:
                            for (int i = 0; i < e.NewItems.Count; i++)
                            {
                                this.collection.RemoveAt(e.OldStartingIndex);
                                this.collection.Insert(e.NewStartingIndex + i, (T)e.NewItems[i]);
                            }
                            break;

                        case NotifyCollectionChangedAction.Remove:
                            for (int i = 0; i < e.OldItems.Count; i++)
                            {
                                this.collection.RemoveAt(e.OldStartingIndex);
                            }
                            break;

                        case NotifyCollectionChangedAction.Replace:
                            for (int i = 0; i < e.NewItems.Count; i++)
                            {
                                this.collection[e.NewStartingIndex + i] = (T)e.NewItems[i];
                            }
                            break;

                        case NotifyCollectionChangedAction.Reset:
                            this.collection.Clear();
                            break;
                    }

                    if (null != CollectionChanged)
                        CollectionChanged(this, e);
                    changes.RemoveAt(0);
                }
            }
            finally
            {
                viewedCollection.Unlock();
            }
        }

        public void Add(T item)
        {
            viewedCollection.Add(item);
            DoSync();
        }

        public T Pop()
        {
            T item = viewedCollection.Pop();
            DoSync();
            return item;
        }

        public void Lock()
        {
            viewedCollection.Lock();
        }

        public void Unlock()
        {
            viewedCollection.Unlock();
        }

        public List<T> ToList()
        {
            return collection.ToList();
        }

        public void Clear()
        {
            viewedCollection.Clear();
            DoSync();
        }

        public bool Contains(T item)
        {
            return viewedCollection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            viewedCollection.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return collection.Count;
            }
        }

        public bool IsReadOnly
        {
            get { return viewedCollection.IsReadOnly; }
        }

        public bool Remove(T item)
        {
            var result = viewedCollection.Remove(item);
            DoSync();
            return result;
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
            return collection.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            viewedCollection.Insert(index, item);
            DoSync();
        }


        public void RemoveAt(int index)
        {
            viewedCollection.RemoveAt(index);
            DoSync();
        }

        public T this[int index]
        {
            get
            {
                return collection[index];
            }
            set
            {
                viewedCollection[index] = value;
                DoSync();
            }
        }

        private void IgnoreWarning()
        {
            PropertyChanged(null, null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Add(object value)
        {
             Add((T)value);
             return IndexOf((T)value);
        }

        public bool Contains(object value)
        {
            return Contains((T)value);
        }

        public int IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        public void Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        public bool IsFixedSize
        {
            get { return ((IList)collection).IsFixedSize; }
        }

        public void Remove(object value)
        {
            Remove((T)value);
        }

        object IList.this[int index]
        {
            get
            {
                return collection[index];
            }
            set
            {
                collection[index] = (T)value;
            }
        }

        public void CopyTo(Array array, int index)
        {
            viewedCollection.Lock();
            for (int i = index; i < collection.Count; i++)
            {
                array.SetValue(viewedCollection[i],i);
            }
            viewedCollection.Unlock();
        }

        public bool IsSynchronized
        {
            get { return ((IList)collection).IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return ((IList)collection).SyncRoot; }
        }
    }
}
