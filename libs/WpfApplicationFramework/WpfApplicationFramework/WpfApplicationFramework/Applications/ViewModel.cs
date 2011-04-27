using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Waf.Foundation;
using System.Windows.Threading;

namespace System.Waf.Applications
{
    /// <summary>
    /// Abstract base class for a ViewModel implementation.
    /// </summary>
    public abstract class ViewModel : Model
    {
        private readonly List<PropertyChangedEventListener> propertyChangedListeners = new List<PropertyChangedEventListener>();
        private readonly List<CollectionChangedEventListener> collectionChangedListeners = new List<CollectionChangedEventListener>();
        private readonly IView view;


        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModel&lt;TView&gt;"/> class and
        /// attaches itself as <c>DataContext</c> to the view.
        /// </summary>
        /// <param name="view">The view.</param>
        protected ViewModel(IView view) : this(view, false) { }


        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModel&lt;TView&gt;"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="isChild">if set to <c>true</c> then this object is a child of another ViewModel.</param>
        protected ViewModel(IView view, bool isChild)
        {
            if (view == null) { throw new ArgumentNullException("view"); }
            this.view = view;
            if (!isChild)
            {
                // Check if the code is running within the WPF application model
                if (SynchronizationContext.Current is DispatcherSynchronizationContext)
                {
                    // Set DataContext of the view has to be delayed so that the ViewModel can initialize the internal data (e.g. Commands)
                    // before the view starts with DataBinding.
                    Dispatcher.CurrentDispatcher.BeginInvoke((Action)delegate()
                    {
                        view.DataContext = this;
                    });
                }
                else
                {
                    // When the code runs outside of the WPF application model then we expect that this constructor is called
                    // within a unit test. Therefore, we don't need the Dispatcher here.
                    view.DataContext = this;
                }
            }
        }


        /// <summary>
        /// Gets the associated view.
        /// </summary>
        public object View { get { return view; } }


        /// <summary>
        /// Adds a weak event listener for a PropertyChanged event.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="handler">The event handler.</param>
        /// <exception cref="ArgumentNullException">source must not be <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">handler must not be <c>null</c>.</exception>
        protected void AddWeakEventListener(INotifyPropertyChanged source, PropertyChangedEventHandler handler)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            if (handler == null) { throw new ArgumentNullException("handler"); }
            
            PropertyChangedEventListener listener = new PropertyChangedEventListener(source, handler);

            propertyChangedListeners.Add(listener);

            PropertyChangedEventManager.AddListener(source, listener, "");
        }

        /// <summary>
        /// Removes the weak event listener for a PropertyChanged event.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="handler">The event handler.</param>
        /// <exception cref="ArgumentNullException">source must not be <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">handler must not be <c>null</c>.</exception>
        protected void RemoveWeakEventListener(INotifyPropertyChanged source, PropertyChangedEventHandler handler)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            if (handler == null) { throw new ArgumentNullException("handler"); }
            
            PropertyChangedEventListener listener = propertyChangedListeners.LastOrDefault(l => 
                l.Source == source && l.Handler == handler);

            if (listener != null)
            {
                propertyChangedListeners.Remove(listener);
                PropertyChangedEventManager.RemoveListener(source, listener, "");
            }
        }

        /// <summary>
        /// Adds a weak event listener for a CollectionChanged event.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="handler">The event handler.</param>
        /// <exception cref="ArgumentNullException">source must not be <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">handler must not be <c>null</c>.</exception>
        protected void AddWeakEventListener(INotifyCollectionChanged source, NotifyCollectionChangedEventHandler handler)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            if (handler == null) { throw new ArgumentNullException("handler"); }
            
            CollectionChangedEventListener listener = new CollectionChangedEventListener(source, handler);

            collectionChangedListeners.Add(listener);

            CollectionChangedEventManager.AddListener(source, listener);
        }

        /// <summary>
        /// Removes the weak event listener for a CollectionChanged event.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="handler">The event handler.</param>
        /// <exception cref="ArgumentNullException">source must not be <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">handler must not be <c>null</c>.</exception>
        protected void RemoveWeakEventListener(INotifyCollectionChanged source, NotifyCollectionChangedEventHandler handler)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            if (handler == null) { throw new ArgumentNullException("handler"); }

            CollectionChangedEventListener listener = collectionChangedListeners.LastOrDefault(l =>
                l.Source == source && l.Handler == handler);

            if (listener != null)
            {
                collectionChangedListeners.Remove(listener);
                CollectionChangedEventManager.RemoveListener(source, listener);
            }
        }
    }
}
