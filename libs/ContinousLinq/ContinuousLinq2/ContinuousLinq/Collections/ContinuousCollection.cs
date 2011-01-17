using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using System.Threading;

namespace ContinuousLinq
{
    /// <summary>
    /// An observable collection that uses the dispatcher thread to pass all notifications of changes
    /// up so that changes can be made to a model object by a worker thread.
    /// This class is taken, almost in it's entirety, from various other implementations of a 
    /// 'thread safe observable collection' available publicly today.
    /// To support console applications, server apps, and services, dispatching is only 
    /// done when there is a valid dispatcher available. In all other cases, the notification
    /// methods are invoked directly.
    /// </summary>
    /// <typeparam name="T">Type of element contained within the collection</typeparam>
    public class ContinuousCollection<T> : ObservableCollection<T>
    {
#if !SILVERLIGHT        
        private readonly Dispatcher _dispatcher;
#endif

        private delegate void ZeroDelegate();
        private delegate void IndexDelegate(int index);
        private delegate void IndexItemDelegate(int index, T item);
        private delegate void IndexIndexDelegate(int oldIndex, int newIndex);

        /// <summary>
        /// Default constructor, initializes a new list and obtains a reference
        /// to the dispatcher.
        /// </summary>
        public ContinuousCollection()
        {
#if !SILVERLIGHT        
            _dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
#endif
        }

#if !SILVERLIGHT        
        /// <summary>
        /// Initializes a new ContinuouseCollection using elements copied from
        /// the specified list and obtains a reference to the dispatcher.
        /// </summary>
        public ContinuousCollection(List<T> list)
            : base(list)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }
#else 
        /// <summary>
        /// Initializes a new ContinuouseCollection using elements copied from
        /// the specified list and obtains a reference to the dispatcher.
        /// </summary>
        public ContinuousCollection(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Add(list[i]);
            }
        }
#endif

        public int BinarySearch(T item)
        {
            List<T> list = (List<T>)this.Items;
            return list.BinarySearch(item);
        }

        public int BinarySearch(T item, IComparer<T> comparer)
        {
            List<T> list = (List<T>)this.Items;
            return list.BinarySearch(item, comparer);
        }

        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            List<T> list = (List<T>)this.Items;
            return list.BinarySearch(index, count, item, comparer);
        }

#if !SILVERLIGHT 
        public void AddRange(IEnumerable<T> collection)
        {
            int index = this.Count;
            this.InsertRange(index, collection);
        }
#endif

#if !SILVERLIGHT   
        public void InsertRange(int index, IEnumerable<T> collection)
        {

            this.CheckReentrancy();

            int indexToInsertAt = index;

            var addedItems = new List<T>();

            foreach (T item in collection)
            {
                addedItems.Add(item);
                this.Items.Insert(indexToInsertAt++, item);
            }

            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            
            this.OnCollectionChanged(NotifyCollectionChangedAction.Add, addedItems, index);
        }
#endif

#if !SILVERLIGHT 
        public void RemoveRange(int index, int count)
        {
  
            this.CheckReentrancy();

            var removedItems = new List<T>();

            for (int i = 0; i < count; i++)
            {
                removedItems.Add(this[index]);
                this.Items.RemoveAt(index);
            }

            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");

            this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItems, index);
        }
#endif

#if !SILVERLIGHT
        public void ReplaceRange(int index, IEnumerable<T> collection)
        {
            this.CheckReentrancy();
            int indexToReplaceAt = index;
            var oldItems = new List<T>();

            foreach (T item in collection)
            {
                oldItems.Add(this[indexToReplaceAt]);
                this.Items[indexToReplaceAt] = item;

                indexToReplaceAt++;
            }

            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");

            var newItems = new List<T>(collection);
            this.OnCollectionReplaced(NotifyCollectionChangedAction.Replace, newItems, oldItems, index);
        }

#endif

#if !SILVERLIGHT
        /// <summary>
        /// If there is no dispatcher, NoInvoke will return true (e.g. Console app)
        /// If there is a dispatcher, and CheckAccess returns true, NoInvoke will return true
        /// If there is a dispatcher, and the call was made from a different thread than the
        /// dispatcher, NoInvoke returns false to indicate that the caller must do
        /// an Invoke on the dispatcher
        /// </summary>
        /// <returns></returns>
        private bool NoInvoke()
        {
            return _dispatcher == null || _dispatcher.CheckAccess();
        }
#else
        private bool NoInvoke()
        {
            return true;
        }
#endif
        /// <summary>
        /// Overridden method that does an "items reset" notification on the dispatch thread
        /// so that bound GUI elements will be aware when this collection is emptied.
        /// </summary>
        protected override void ClearItems()
        {
            if (NoInvoke())
            {
                base.ClearItems();
            }
            else
            {
#if !SILVERLIGHT   
                _dispatcher.Invoke(DispatcherPriority.Normal, new ZeroDelegate(ClearItems));
#endif
            }
        }

        /// <summary>
        /// Overridden method that inserts and item into the base collection. Performs a thread-safe
        /// write to the collection, and dispatches the change notification onto the dispatcher thread
        /// </summary>
        /// <param name="index">Index of item being inserted</param>
        /// <param name="item">Item being inserted</param>
        protected override void InsertItem(int index, T item)
        {
            if (NoInvoke())
            {
                base.InsertItem(index, item);
            }
            else
            {
#if !SILVERLIGHT   
                _dispatcher.Invoke(DispatcherPriority.Normal,
                    new IndexItemDelegate(InsertItem), index, item);
#endif
            }
        }
        

#if !SILVERLIGHT
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            if (NoInvoke())
            {
                base.MoveItem(oldIndex, newIndex);
            }
            else
            {
                _dispatcher.Invoke(DispatcherPriority.Normal,
                    new IndexIndexDelegate(MoveItem), oldIndex, newIndex);
            }
        }
#endif
        /// <summary>
        /// Overridden method that performs a thread-safe removal of the item from the collection. Also sends 
        /// the notification of that removal to the Dispatcher thread.
        /// </summary>
        /// <param name="index">Index of item to be removed.</param>
        protected override void RemoveItem(int index)
        {
            if (NoInvoke())
            {
                //    AddOrRemovePropertyListener(this[index], false);
                base.RemoveItem(index);
            }
            else
            {
#if !SILVERLIGHT 
                _dispatcher.Invoke(DispatcherPriority.Normal, new IndexDelegate(RemoveItem), index);
#endif
            }

        }

        /// <summary>
        /// Overridden method that performs a thread-safe replacement of an item within the collection.
        /// </summary>
        /// <param name="index">Index of the item to set</param>
        /// <param name="item">Item being set</param>
        protected override void SetItem(int index, T item)
        {
            if (NoInvoke())
            {
                base.SetItem(index, item);
            }
            else
            {
#if !SILVERLIGHT 
                _dispatcher.Invoke(DispatcherPriority.Normal,
                    new IndexItemDelegate(SetItem), index, item);
#endif
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

#if !SILVERLIGHT 
        private void OnCollectionChanged(NotifyCollectionChangedAction action, IList list, int index)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, list, index));
        }

        private void OnCollectionReplaced(NotifyCollectionChangedAction action, IList newItems, IList oldItems, int index)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItems, oldItems, index));
        }
#endif
    }
}

