using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;

namespace ContinuousLinq
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(ReadOnlyContinuousCollection<>.DebugView))]
    public abstract class ReadOnlyContinuousCollection<T> : INotifyCollectionChanged, INotifyPropertyChanged, IList<T>, IList
    {
        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        private void FireCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, args);
            }

            if (args.Action != NotifyCollectionChangedAction.Replace)
            {
                OnPropertyChanged("Count");
            }
        }

        protected void RefireCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            FireCollectionChanged(args);
        }

        protected void FireReset()
        {
            FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected void FireAdd(IEnumerable<T> newItems, int startingIndex)
        {
#if SILVERLIGHT
            int lastIndex = startingIndex;
            foreach (var item in newItems)
            {
                FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, lastIndex));
                lastIndex++;
            }
#else
            FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems as IList ?? newItems.ToList(), startingIndex));
#endif
        }

        protected void FireAddList(IList newItems, int startingIndex)
        {
#if SILVERLIGHT
            for (int i = 0; i < newItems.Count; i++)
            {
                FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems[i], startingIndex + i));
            }
#else
            FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, startingIndex));
#endif
        }

        protected void FireAddItem(T item, int startingIndex)
        {
            FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, startingIndex));
        }


        protected void FireRemove(IEnumerable<T> oldItems, int startingIndex)
        {
#if SILVERLIGHT

            int lastIndex = startingIndex;
            foreach (var item in oldItems)
            {
                FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, lastIndex));
                lastIndex++;
            }
#else
            FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems as IList ?? oldItems.ToList(), startingIndex));
#endif
        }

        protected void FireRemoveList(IList oldItems, int startingIndex)
        {
#if SILVERLIGHT
            for (int i = 0; i < oldItems.Count; i++)
            {
                FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems[i], startingIndex + i));
            }
#else
            FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, startingIndex));
#endif
        }

        protected void FireRemoveItem(T item, int startingIndex)
        {
            FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, startingIndex));
        }

        protected void FireReplace(IEnumerable<T> newItems, IEnumerable<T> oldItems, int startingIndex)
        {
#if SILVERLIGHT
            int currentIndexInCollection = startingIndex;

            var newItemEnumerator = newItems.GetEnumerator();
            var oldItemEnumerator = oldItems.GetEnumerator();

            bool hasMoreItemsLeftInNew = newItemEnumerator.MoveNext();
            bool hasMoreItemsLeftInOld = oldItemEnumerator.MoveNext();
            while(hasMoreItemsLeftInNew && hasMoreItemsLeftInOld)
            {
                FireReplaceItem(newItemEnumerator.Current, oldItemEnumerator.Current, currentIndexInCollection++);
                hasMoreItemsLeftInNew = newItemEnumerator.MoveNext();
                hasMoreItemsLeftInOld = oldItemEnumerator.MoveNext();
            }

            while (hasMoreItemsLeftInNew)
            {
                FireAddItem(newItemEnumerator.Current, currentIndexInCollection++);
                hasMoreItemsLeftInNew = newItemEnumerator.MoveNext();
            }

            while (hasMoreItemsLeftInOld)
            {
                FireRemoveItem(oldItemEnumerator.Current, currentIndexInCollection++);
                hasMoreItemsLeftInOld = oldItemEnumerator.MoveNext();
            }

#else
            FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems as IList ?? newItems.ToList(), oldItems as IList ?? oldItems.ToList(), startingIndex));
#endif

        }

        protected void FireReplaceList(IList newItems, IList oldItems, int startingIndex)
        {
#if SILVERLIGHT
            int indexInOldItems = 0;
            int indexInNewItems = 0;

            int currentIndexInCollection = startingIndex;

            while (indexInNewItems < newItems.Count && indexInOldItems < oldItems.Count)
            {
                FireReplaceItem((T)newItems[indexInNewItems++], (T)oldItems[indexInOldItems++], currentIndexInCollection++);
            }

            while (indexInNewItems < newItems.Count)
            {
                FireAddItem((T)newItems[indexInNewItems++], currentIndexInCollection++);
            }

            while (indexInOldItems < oldItems.Count)
            {
                FireRemoveItem((T)oldItems[indexInOldItems++], currentIndexInCollection++);
            }

#else
            FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems, startingIndex));
#endif

        }

        protected void FireReplaceItem(T newItem, T oldItem, int startingIndex)
        {
            FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, startingIndex));
        }

#if !SILVERLIGHT
        protected void FireMove(IList newItems, int newStartingIndex, int oldStartingIndex)
        {
            FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, newItems, newStartingIndex, oldStartingIndex));
        }
#else
        protected void FireMove(IList newItems, int newStartingIndex, int oldStartingIndex)
        {
            throw new NotImplementedException("Silverlight does not support move operations.");
        }
#endif
        #region IList<T> Members

        public int IndexOf(T item)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(this[i], item))
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new AccessViolationException();
        }

        public void RemoveAt(int index)
        {
            throw new AccessViolationException();
        }

        public abstract T this[int index] { get; set; }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            throw new AccessViolationException();
        }

        public void Clear()
        {
            throw new AccessViolationException();
        }

        public bool Contains(T item)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(this[i], item))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            int currentArrayIndex = arrayIndex;
            for (int i = 0; i < this.Count; i++, arrayIndex++)
            {
                array[arrayIndex] = this[i];
            }
        }

        public abstract int Count
        {
            get;
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(T item)
        {
            throw new AccessViolationException();
        }

        #endregion

        #region IEnumerable<T> Members

        public virtual IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < this.Count; i++)
            {
                yield return this[i];
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IList Members

        public int Add(object value)
        {
            throw new AccessViolationException();
        }

        private static bool CanUseGenericVersionOfMethod(object value)
        {
            return (value is T) || (typeof(T).IsValueType && value != null);
        }

        public bool Contains(object value)
        {
            if (CanUseGenericVersionOfMethod(value))
            {
                return Contains((T)value);
            }

            return false;
        }

        public int IndexOf(object value)
        {
            if (CanUseGenericVersionOfMethod(value))
            {
                return IndexOf((T)value);
            }
            return -1;
        }

        public void Insert(int index, object value)
        {
            throw new AccessViolationException();
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public void Remove(object value)
        {
            throw new AccessViolationException();
        }

        object IList.this[int index]
        {
            get
            {
                return ((IList<T>)this)[index];
            }
            set
            {
                throw new AccessViolationException();
            }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int arrayIndex)
        {
            for (int i = 0; i < this.Count; i++, arrayIndex++)
            {
                array.SetValue(this[i], arrayIndex);
            }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return this; }
        }

        #endregion

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
   
        internal class DebugView
        {
            private ReadOnlyContinuousCollection<T> Collection { get; set; }

            public DebugView(ReadOnlyContinuousCollection<T> collection)
            {
                this.Collection = collection;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public T[] Items
            {
                get
                {
                    T[] items = new T[this.Collection.Count];
                    this.Collection.CopyTo(items, 0);

                    return items;
                }
            }
        }
    }
}