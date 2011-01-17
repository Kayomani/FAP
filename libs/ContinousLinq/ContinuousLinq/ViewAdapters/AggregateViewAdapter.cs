using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace ContinuousLinq.Aggregates
{
    public abstract class AggregateViewAdapter
    {
        protected bool _localPaused = false;
        protected static bool _globalPaused = false;
        protected static List<AggregateViewAdapter> _dirtiedSincePause = new List<AggregateViewAdapter>();        

        public static void GlobalPause()
        {
            _globalPaused = true;
        }

        public static void GlobalResume()
        {
            _globalPaused = false;
            ResumeDirtyValues();
        }

        public void Pause() // LOCAL PAUSE
        {
            _localPaused = true;            
        }

        public void Resume() // LOCAL RESUME
        {
            _localPaused = false;
            if (_dirtiedSincePause.Contains(this))
            {
                _dirtiedSincePause.Remove(this);
                ReAggregate();
            }
            
        }

        private static void ResumeDirtyValues()
        {
            foreach (AggregateViewAdapter ava in _dirtiedSincePause)
            {
                ava.Resume();
            }
            _dirtiedSincePause.Clear();
        }

        public static bool IsGloballyPaused
        {
            get
            {
                return _globalPaused;
            }
        }

        public bool IsPaused
        {
            get
            {
                return _localPaused;
            }
        }

        protected abstract void ReAggregate();
    }

    public abstract class AggregateViewAdapter<Tinput, Toutput> : 
        AggregateViewAdapter, System.Windows.IWeakEventListener where Tinput : INotifyPropertyChanged
    {
        private readonly InputCollectionWrapper<Tinput> _input;        
        private readonly ContinuousValue<Toutput> _output = new ContinuousValue<Toutput>();

        protected HashSet<string> _monitoredProperties;
        private bool _collectionOnlyMonitor = false;
        private bool _paused = false;

        protected AggregateViewAdapter(ObservableCollection<Tinput> input, 
            HashSet<string> monitoredProperties,
            bool collectionOnly) :
            this(new InputCollectionWrapper<Tinput>(input), monitoredProperties, collectionOnly)
        {
        }

        protected AggregateViewAdapter(ReadOnlyObservableCollection<Tinput> input,
            HashSet<string> monitoredProperties,
            bool collectionOnly) :
            this(new InputCollectionWrapper<Tinput>(input), monitoredProperties, collectionOnly)
        {
        }

        private AggregateViewAdapter(InputCollectionWrapper<Tinput> input,
            HashSet<string> monitoredProperties,
            bool collectionOnly)
        {
            _collectionOnlyMonitor = collectionOnly;
            _monitoredProperties = monitoredProperties;
            _input = input;
            _output.SourceAdapter = this;                        

            foreach (Tinput item in this.InputCollection)
            {
                SubscribeToItem(item);
            }

            CollectionChangedEventManager.AddListener(
                _input.InnerAsNotifier, this);
            // Subclasses must call ReAggregate here!!!
        }

        protected IList<Tinput> Input
        {
            get { return _input.InnerAsList; }
        }

        protected void SubscribeToItem(Tinput item)
        {
            if (_collectionOnlyMonitor) return;

            if (_monitoredProperties == null || _monitoredProperties.Count == 0)
                PropertyChangedEventManager.AddListener(item, this, string.Empty);

            else
            {
                foreach (string monitoredProperty in _monitoredProperties)
                {
                    PropertyChangedEventManager.AddListener(
                        item,
                        this,
                        monitoredProperty);
                }
            }
        }

        protected void UnsubscribeFromItem(Tinput item)
        {
            if (_collectionOnlyMonitor) return;

            if (_monitoredProperties == null || _monitoredProperties.Count == 0)
                PropertyChangedEventManager.RemoveListener(item, this, string.Empty);

            else
            {
                foreach (string monitoredProperty in _monitoredProperties)
                {
                    PropertyChangedEventManager.RemoveListener(
                        item,
                        this,
                        monitoredProperty);
                }
            }
        }

        /// <summary>
        /// A pointer to the collection to which this adapter is listening
        /// </summary>
        protected IList<Tinput> InputCollection
        {
            get { return _input.InnerAsList; }
        }        

        public ContinuousValue<Toutput> Value
        {
            get { return _output; }
        }

        protected void SetCurrentValue(Toutput newvalue)
        {
            _output.CurrentValue = newvalue;
        }

        protected void SetCurrentValueToDefault()
        {
            _output.CurrentValue = default(Toutput);
        }

        //protected abstract void ReAggregate();        

        #region IWeakEventListener Members

        public bool ReceiveWeakEvent(System.Type managerType, object sender, System.EventArgs e)
        {
            if (_globalPaused || _localPaused)
            {
                _dirtiedSincePause.Add(this);
            }
            if (managerType == typeof(PropertyChangedEventManager))
            {
                if (!_localPaused && !_globalPaused)
                    ReAggregate();
            }
            else
            {
                // collection changed event.
                NotifyCollectionChangedEventArgs collectionArgs =
                    (NotifyCollectionChangedEventArgs)e;
                switch (collectionArgs.Action)
                {
                    case NotifyCollectionChangedAction.Remove:
                        Tinput toDelete = (Tinput)collectionArgs.OldItems[0];
                        UnsubscribeFromItem(toDelete);                        
                        break;
                    case NotifyCollectionChangedAction.Add:
                        Tinput toAdd = (Tinput)collectionArgs.NewItems[0];
                        SubscribeToItem(toAdd);                        
                        break;
                    case NotifyCollectionChangedAction.Replace:
                    case NotifyCollectionChangedAction.Move:
                        UnsubscribeFromItem((Tinput)collectionArgs.OldItems[0]);
                        SubscribeToItem((Tinput)collectionArgs.NewItems[0]);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        foreach (Tinput item in this.InputCollection)
                        {
                            UnsubscribeFromItem(item);
                        }                        
                        break;
                }

                if (!_localPaused && !_globalPaused)
                    ReAggregate();
            }

            return true;
        }

        #endregion    
    }
}
