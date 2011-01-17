using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Collections.Specialized;
using System.Diagnostics;

namespace ContinuousLinq
{
    internal abstract class SmartViewAdapter<TInput, TOutput> : IViewAdapter,
                                                                IWeakEventListener 
        where TInput : INotifyPropertyChanged
    {
        protected HashSet<string> _monitoredProperties;

        private readonly InputCollectionWrapper<TInput> _input;
        private readonly OutputCollectionWrapper<TOutput> _output;

#if DEBUG
        public int PropertyChangeNotifyCount { get; set; }
#endif


        protected SmartViewAdapter(InputCollectionWrapper<TInput> input,
            LinqContinuousCollection<TOutput> output,
            HashSet<string> monitoredProperties)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (output == null)
                throw new ArgumentNullException("output");

            _input = input;
            _output = new OutputCollectionWrapper<TOutput>(output);
            _monitoredProperties = monitoredProperties;

            output.SourceAdapter = this;
            // Must be referenced by this instance:
           
            foreach (TInput item in _input.InnerAsList)
            {
                SubscribeToItem(item);
            }

            CollectionChangedEventManager.AddListener(
                _input.InnerAsNotifier, this);
        }

        private void SubscribeToItem(TInput item)
        {
            if (_monitoredProperties == null || _monitoredProperties.Count == 0)
                PropertyChangedEventManager.AddListener(item, this, string.Empty);

            foreach (string monitoredProperty in _monitoredProperties)
            {
                PropertyChangedEventManager.AddListener(
                    item,
                    this,
                    monitoredProperty);                
            }
        }

        private void UnsubscribeFromItem(TInput item)
        {
            if (_monitoredProperties == null || _monitoredProperties.Count == 0)
                PropertyChangedEventManager.RemoveListener(item, this, string.Empty);

            foreach (string monitoredProperty in _monitoredProperties)
            {
                PropertyChangedEventManager.RemoveListener(
                    item,
                    this,
                    monitoredProperty);
            }
        }

        protected abstract void ItemAdded(TInput item);
        protected abstract bool ItemRemoved(TInput item);
        protected abstract void ItemPropertyChanged(TInput item);
        protected abstract void ItemsCleared();
        public abstract void ReEvaluate();                

        #region IViewAdapter Members

        public IViewAdapter PreviousAdapter
        {
            get { return _input.SourceAdapter; }
        }        

        #endregion

        #region IWeakEventListener Members

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {            
            if (managerType == typeof(PropertyChangedEventManager))
            {                
#if DEBUG
               PropertyChangeNotifyCount++;
#endif
                ItemPropertyChanged((TInput)sender);
            }
            else
            {
                // collection changed event.
                NotifyCollectionChangedEventArgs collectionArgs =
                    (NotifyCollectionChangedEventArgs)e;
                switch (collectionArgs.Action)
                {
                    case NotifyCollectionChangedAction.Remove:
                        TInput toDelete = (TInput)collectionArgs.OldItems[0];
                        UnsubscribeFromItem(toDelete);
                        ItemRemoved(toDelete);
                        break;
                    case NotifyCollectionChangedAction.Add:
                        TInput toAdd = (TInput)collectionArgs.NewItems[0];
                        SubscribeToItem(toAdd);
                        ItemAdded(toAdd);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                    case NotifyCollectionChangedAction.Move:
                        TInput oldItem = (TInput)collectionArgs.OldItems[0];
                        UnsubscribeFromItem(oldItem);
                        if (ItemRemoved(oldItem))
                        {
                            TInput newItem = (TInput)collectionArgs.NewItems[0];
                            SubscribeToItem(newItem);
                            ItemAdded(newItem);
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        foreach (TInput item in this.InputCollection)
                        {
                            UnsubscribeFromItem(item);
                        }
                        ItemsCleared();
                        break;
                }
                

                this.ReEvaluate();
            }

            return true;
        }

        #endregion

        /// <summary>
        /// A pointer to the collection to which this adapter is listening
        /// </summary>
        internal IList<TInput> InputCollection
        {
            get { return _input.InnerAsList; }
        }

        /// <summary>
        /// A pointer to the collection to which all changes made by this adapter are
        /// propagated
        /// </summary>
        internal OutputCollectionWrapper<TOutput> OutputCollection
        {
            get { return _output; }
        }

    }
}
