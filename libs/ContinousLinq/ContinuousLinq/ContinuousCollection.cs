using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Threading;
using System.ComponentModel;

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
        // Maybe later this will be changeable:
        private readonly DispatcherPriority _updatePriority = DispatcherPriority.Normal;
        private readonly Dispatcher _dispatcher;
        

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
            _dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
        }

        /// <summary>
        /// Initializes a new ContinuouseCollection using elements copied from
        /// the specified list and obtains a reference to the dispatcher.
        /// </summary>
        public ContinuousCollection(List<T> list)
            : base(list)
        {         
            _dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
        }

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

        public void AddRange(IEnumerable<T> collection)
        {
            foreach (T item in collection)
            {
                Add(item);
            }
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            foreach (T item in collection)
            {
                Insert(index++, item);
            }
        }

        public void RemoveRange(int index, int count)
        {
            for (; count > 0; count--)
            {
                this.RemoveAt(index);
            }
        }

        public void ReplaceRange(int index, IEnumerable<T> collection)
        {
            foreach (T item in collection)
            {
                this[index++] = item;
            }
        }

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
                _dispatcher.Invoke(_updatePriority, new ZeroDelegate(ClearItems));
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
                _dispatcher.Invoke(_updatePriority,
                    new IndexItemDelegate(InsertItem), index, item);
            }
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            if (NoInvoke())
            {
                base.MoveItem(oldIndex, newIndex);
            }
            else
            {
                _dispatcher.Invoke(_updatePriority,
                    new IndexIndexDelegate(MoveItem), oldIndex, newIndex);
            }
        }

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
                _dispatcher.Invoke(_updatePriority, new IndexDelegate(RemoveItem), index);
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
                _dispatcher.Invoke(_updatePriority,
                    new IndexItemDelegate(SetItem), index, item);
            }
        }
    }
}
