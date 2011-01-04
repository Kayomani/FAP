using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace Fap.Foundation
{
    /// <summary>
    /// This class is used to solve the issue where you cannot update an observable collection from a background
    /// thread. See: http://geekswithblogs.net/NewThingsILearned/archive/2008/01/16/have-worker-thread-update-observablecollection-that-is-bound-to-a.aspx
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        // Override the event so this class can access it
        public override event System.Collections.Specialized.NotifyCollectionChangedEventHandler CollectionChanged;

        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Be nice - use BlockReentrancy like MSDN said
            using (BlockReentrancy())
            {
                System.Collections.Specialized.NotifyCollectionChangedEventHandler eventHandler = CollectionChanged;
                if (eventHandler == null)
                    return;

                Delegate[] delegates = eventHandler.GetInvocationList();
                // Walk thru invocation list
                foreach (System.Collections.Specialized.NotifyCollectionChangedEventHandler handler in delegates)
                {
                    DispatcherObject dispatcherObject = handler.Target as DispatcherObject;
                    // If the subscriber is a DispatcherObject and different thread
                    if (dispatcherObject != null && dispatcherObject.CheckAccess() == false)
                    {
                        // Invoke handler in the target dispatcher's thread
                        dispatcherObject.Dispatcher.Invoke(DispatcherPriority.DataBind, handler, this, e);
                    }
                    else // Execute handler as is
                        handler(this, e);
                }
            }
        }

        public void TriggerCollectionChanged()
        {
            if (null != CollectionChanged)
                CollectionChanged(this, new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
        }
    }
}
