using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Collections.Specialized;
using System.Collections;
using System.Linq.Expressions;
using ContinuousLinq.WeakEvents;

namespace ContinuousLinq
{
    public class NotifyCollectionChangedMonitor<T> : INotifyCollectionChanged
    {
        private IPropertyAccessTreeSubscriber<NotifyCollectionChangedMonitor<T>> _propertyChangedSubscription;
        protected readonly IList<T> _input;

        public PropertyAccessTree PropertyAccessTree { get; set; }

        public bool IsMonitoringChildProperties
        {
            get { return this.PropertyAccessTree != null && this.PropertyAccessTree.IsMonitoringChildProperties; }
        }

        internal Dictionary<T, SubscriptionTree> Subscriptions { get; set; }

        public ReferenceCountTracker<T> ReferenceCountTracker { get; set; }

        public event Action<object, int, IEnumerable<T>> Add;
        public event Action<object, int, IEnumerable<T>> Remove;
        public event Action<object, IEnumerable<T>, int, IEnumerable<T>> Replace;
        public event Action<object, int, IEnumerable<T>, int, IEnumerable<T>> Move;
        public event Action<object> Reset;

        public event Action<object, INotifyPropertyChanged> ItemChanged;

        #region IWeakEventListener Members

        public static NotifyCollectionChangedMonitor<T> Create<TResult>(IList<T> input, Expression<Func<T, TResult>> expression)
        {
            return new NotifyCollectionChangedMonitor<T>(ExpressionPropertyAnalyzer.Analyze(expression), input);
        }

        public NotifyCollectionChangedMonitor(PropertyAccessTree propertyAccessTree, IList<T> input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            INotifyCollectionChanged inputAsINotifyCollectionChanged = input as INotifyCollectionChanged;

            if (inputAsINotifyCollectionChanged == null)
                throw new ArgumentException("Collections must implement INotifyCollectionChanged to work with CLINQ");

            _input = input;

            this.PropertyAccessTree = propertyAccessTree;

            this.ReferenceCountTracker = new ReferenceCountTracker<T>();

            SetupPropertyChangeSubscription();

            SubscribeToItems(_input);

            WeakNotifyCollectionChangedEventHandler.Register(
                inputAsINotifyCollectionChanged,
                this,
                (me, sender, args) => me.OnCollectionChanged(sender, args));
        }

        #region PropertyChangeSubscriptions
        
        private void SetupPropertyChangeSubscription()
        {
            if (this.PropertyAccessTree == null)
                return;

            if (this.PropertyAccessTree.DoesEntireTreeSupportINotifyPropertyChanging)
            {
                _propertyChangedSubscription = this.PropertyAccessTree.CreateCallbackSubscription<NotifyCollectionChangedMonitor<T>>(OnAnyPropertyChange);
            }
            else
            {
                this.Subscriptions = new Dictionary<T, SubscriptionTree>();
            }
        }

        private void SubscribeToItems(IList items)
        {
            if (this.PropertyAccessTree == null)
                return;

            for (int i = 0; i < items.Count; i++)
            {
                SubscribeToItem((T)items[i]);
            }
        }

        private void SubscribeToItems(IList<T> items)
        {
            if (this.PropertyAccessTree == null)
                return;

            for (int i = 0; i < items.Count; i++)
            {
                SubscribeToItem(items[i]);
            }
        }

        private void SubscribeToItem(T item)
        {
            bool wasFirstTimeAddedToCollection = this.ReferenceCountTracker.Add(item);
            if (!wasFirstTimeAddedToCollection || !this.IsMonitoringChildProperties)
                return;
            
            if (_propertyChangedSubscription != null)
            {
                _propertyChangedSubscription.SubscribeToChanges((INotifyPropertyChanged)item, this);
            }
            else
            {
                SubscriptionTree subscriptionTree = this.PropertyAccessTree.CreateSubscriptionTree((INotifyPropertyChanged)item);
                subscriptionTree.PropertyChanged += OnAnyPropertyChangeInSubscriptionTree;
                this.Subscriptions.Add(item, subscriptionTree);
            }
        }

        static void OnAnyPropertyChange(NotifyCollectionChangedMonitor<T> monitor, object itemThatChanged)
        {
            if (monitor.ItemChanged == null)
                return;

            monitor.ItemChanged(monitor, (INotifyPropertyChanged)itemThatChanged);
        }

        void OnAnyPropertyChangeInSubscriptionTree(SubscriptionTree sender)
        {
            OnAnyPropertyChange(this, sender.Parameter);
        }

        private void UnsubscribeFromItems(IList items)
        {
            if (this.PropertyAccessTree == null)
                return;

            for (int i = 0; i < items.Count; i++)
            {
                UnsubscribeFromItem((T)items[i]);
            }
        }

        private void UnsubscribeFromItem(T item)
        {
            bool wasLastInstanceOfItemRemovedFromCollection = this.ReferenceCountTracker.Remove(item);
            if (!wasLastInstanceOfItemRemovedFromCollection || !this.IsMonitoringChildProperties)
                return;

            if (_propertyChangedSubscription != null)
            {
                _propertyChangedSubscription.UnsubscribeFromChanges((INotifyPropertyChanged)item, this);
            }
            else
            {
                this.Subscriptions[item].PropertyChanged -= OnAnyPropertyChangeInSubscriptionTree;
                this.Subscriptions.Remove(item);
            }
        }

        private void ClearSubscriptions()
        {
            if (this.PropertyAccessTree == null)
                return;

            if (this.IsMonitoringChildProperties)
            {
                if (_propertyChangedSubscription != null)
                {
                    foreach (T item in this.ReferenceCountTracker.Items)
                    {
                        _propertyChangedSubscription.UnsubscribeFromChanges((INotifyPropertyChanged)item, this);
                    }
                }
                else
                {
                    foreach (var subscriptionTree in this.Subscriptions.Values)
                    {
                        subscriptionTree.PropertyChanged -= OnAnyPropertyChangeInSubscriptionTree;
                    }
                }
            }
            this.ReferenceCountTracker.Clear();
        }

        #endregion

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs ea)
        {
            ReceiveCollectionChangedEvent(ea);
        }

        private void ReceiveCollectionChangedEvent(EventArgs e)
        {
            NotifyCollectionChangedEventArgs collectionArgs = (NotifyCollectionChangedEventArgs)e;
            switch (collectionArgs.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    UnsubscribeFromItems(collectionArgs.OldItems);
                    if (Remove != null)
                    {
                        Remove(this, collectionArgs.OldStartingIndex, collectionArgs.OldItems.Cast<T>());
                    }
                    break;
                case NotifyCollectionChangedAction.Add:
                    SubscribeToItems(collectionArgs.NewItems);
                    if (Add != null)
                    {
                        Add(this, collectionArgs.NewStartingIndex, collectionArgs.NewItems.Cast<T>());
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    UnsubscribeFromItems(collectionArgs.OldItems);
                    SubscribeToItems(collectionArgs.NewItems);
                    if (Replace != null)
                    {
                        Replace(this, 
                            collectionArgs.OldItems.Cast<T>(),
                            collectionArgs.NewStartingIndex,
                            collectionArgs.NewItems.Cast<T>());
                    }
                    break;
#if !SILVERLIGHT   
                case NotifyCollectionChangedAction.Move:
                    if (Move != null)
                    {
                        Move(this,
                            collectionArgs.OldStartingIndex,
                            collectionArgs.OldItems.Cast<T>(),
                            collectionArgs.NewStartingIndex,
                            collectionArgs.NewItems.Cast<T>());
                    }
                    break;
#endif
                case NotifyCollectionChangedAction.Reset:
                    ClearSubscriptions();
                    SubscribeToItems(_input);
                    if (Reset != null)
                    {
                        Reset(this);
                    }
                    break;
            }
            if (CollectionChanged != null)
            {
                CollectionChanged(this, collectionArgs);
            }
        }
        #endregion

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion
    }
}
