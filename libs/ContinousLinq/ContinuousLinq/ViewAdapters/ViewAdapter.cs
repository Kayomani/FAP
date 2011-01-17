using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.Specialized;

namespace ContinuousLinq
{   

    /// We want to arrange things so that when the output collection of
    /// a CLINQ chain gets dropped, all the intermediate adapters and
    /// collections also get dropped.  So we use weak event listeners.
    /// But we don't want the intermediate stuff to be dropped too early,
    /// so we need hard links back from the output collection to its
    /// adapter (which already has a hard link to its input collection).
    /// This hard link is the collection's SourceAdapter, which is also
    /// used for other purposes.
    internal abstract class ViewAdapter<Tin, Tout> : IViewAdapter
        where Tin : INotifyPropertyChanged
    {
        private readonly InputCollectionWrapper<Tin> _input;
        private readonly OutputCollectionWrapper<Tout> _output;
        private readonly NotifyCollectionChangedEventHandler _collectionChangedDelegate;
        private readonly PropertyChangedEventHandler _propertyChangedDelegate;
        private readonly Dictionary<Tin, WeakPropertyChangedHandler> _handlerMap = 
            new Dictionary<Tin, WeakPropertyChangedHandler>();

        protected ViewAdapter(InputCollectionWrapper<Tin> input,
            LinqContinuousCollection<Tout> output)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (output == null)
                throw new ArgumentNullException("output");

            _input = input;
            _output = new OutputCollectionWrapper<Tout>(output);

            output.SourceAdapter = this;
            // Must be referenced by this instance:
            _collectionChangedDelegate = delegate(object sender, NotifyCollectionChangedEventArgs args)
                                             {
                                                 OnInputCollectionChanged(args);
                                             };
            _propertyChangedDelegate = delegate(object sender, PropertyChangedEventArgs args)
                                           {
                                               OnCollectionItemPropertyChanged((Tin)sender, args.PropertyName);
                                           };
            new WeakCollectionChangedHandler(input.InnerAsNotifier, _collectionChangedDelegate);

            foreach (Tin item in this.InputCollection)
            {
                SubscribeToItem(item);
            }
        }

        /// <summary>
        /// Subscribes to change events fired by the input item using a weak event handler, 
        /// only if the item has not been added to the handler map yet.
        /// </summary>
        /// <param name="item"></param>
        protected void SubscribeToItem(Tin item)
        {
            if (_handlerMap.ContainsKey(item) == false)
            {
                _handlerMap[item] = new WeakPropertyChangedHandler(item, _propertyChangedDelegate);
            }
        }


        /// <summary>
        /// Unplugs all change event monitors (weak) from the indicated item.
        /// </summary>
        /// <param name="item"></param>
        protected void UnsubscribeFromItem(Tin item)
        {
            WeakPropertyChangedHandler handler;
            if (_handlerMap.TryGetValue(item, out handler))
            {
                handler.Detach();
                _handlerMap.Remove(item);
            }
        }

        public IViewAdapter PreviousAdapter
        {
            get { return _input.SourceAdapter; }
        }

        /// <summary>
        /// A pointer to the collection to which this adapter is listening
        /// </summary>
        internal IList<Tin> InputCollection
        {
            get { return _input.InnerAsList; }
        }

        /// <summary>
        /// A pointer to the collection to which all changes made by this adapter are
        /// propagated
        /// </summary>
        internal OutputCollectionWrapper<Tout> OutputCollection
        {
            get { return _output; }
        }

        /// <summary>
        /// Defined in derived types
        /// </summary>
        /// <param name="item"></param>
        /// <param name="propertyName"></param>
        protected abstract void OnCollectionItemPropertyChanged(Tin item, string propertyName);

        /// <summary>
        /// Defined by derived types to respond to the event in which an item in the source collection
        /// is added.
        /// </summary>
        /// <param name="newItem"></param>
        /// <param name="index"></param>
        protected abstract void AddItem(Tin newItem, int index);

        /// <summary>
        /// Defined by derived types to respond to the event
        /// </summary>
        /// <param name="newItem"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected abstract bool RemoveItem(Tin newItem, int index);

        protected abstract void Clear();

        /// <summary>
        /// Base actions to take place when the collection being monitored notifies the adapter
        /// of a change. This method calls AddItem and RemoveItem, which are defined by the derived
        /// types for cleanliness.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnInputCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    Tin toDelete = (Tin)e.OldItems[0];
                    UnsubscribeFromItem(toDelete);
                    RemoveItem(toDelete, e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Add:
                    Tin toAdd = (Tin)e.NewItems[0];
                    SubscribeToItem(toAdd);
                    AddItem(toAdd, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    Tin oldItem = (Tin)e.OldItems[0];
                    UnsubscribeFromItem(oldItem);
                    if (RemoveItem(oldItem, e.OldStartingIndex))
                    {
                        Tin newItem = (Tin)e.NewItems[0];
                        SubscribeToItem(newItem);
                        AddItem(newItem, e.NewStartingIndex);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (Tin item in this.InputCollection)
                    {
                        UnsubscribeFromItem(item);
                    }
                    Clear();
                    break;
            }
        }

        /// <summary>
        /// Any adapters with lambda expressions need to refilter now.
        /// Force subclasses to implement so we don't forget any.
        /// Public because of the interface.
        /// </summary>
        public abstract void ReEvaluate();
      
    }
}
