using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ContinuousLinq.WeakEvents
{
    public class WeakPropertyBridge
    {
        private INotifyPropertyChanged _source;
        private HybridDictionary _propertyNameToCallbacks;

        public bool HasActiveListeners
        {
            get { return _propertyNameToCallbacks.Count > 0; }
        }

        public WeakPropertyBridge(INotifyPropertyChanged source)
        {
            _source = source;
            _propertyNameToCallbacks = new HybridDictionary();

            source.PropertyChanged += OnPropertyChanged;

            INotifyPropertyChanging sourceAsINotifyPropertyChanging = source as INotifyPropertyChanging;
            if (sourceAsINotifyPropertyChanging != null)
            {
                sourceAsINotifyPropertyChanging.PropertyChanging += OnPropertyChanging;
            }
        }

        public WeakPropertyBridge(INotifyPropertyChanged rootSource, INotifyPropertyChanged source)
            : this(source)
        {
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            ForEachCallback(args.PropertyName, (callbacksForProperty, currentNode) =>
            {
                IWeakEventCallback<PropertyChangedEventArgs> callback = (IWeakEventCallback<PropertyChangedEventArgs>)currentNode.Value;

                if (!callback.Invoke(sender, args))
                {
                    callbacksForProperty.MarkForRemove(currentNode);
                }
                return true;
            });
            //CheckForUnsubscribe(callbacksForProperty, args.PropertyName);
        }

        public void RemoveListener(object listener, string propertyName, object rootSubject)
        {
            ForEachCallback(propertyName, (callbacksForProperty, currentNode) =>
            {
                IWeakEventCallback<PropertyChangedEventArgs> callback = (IWeakEventCallback<PropertyChangedEventArgs>)currentNode.Value;
                object listenerForCallback = callback.ListenerReference.Target;
                object rootSubjectForCallback = callback.RootSubject.Target;
 
                if (listenerForCallback == null)
                {
                    callbacksForProperty.MarkForRemove(currentNode);
                }
                else if (listenerForCallback == listener && rootSubject == rootSubjectForCallback)
                {
                    callbacksForProperty.MarkForRemove(currentNode);
                    return false;
                }

                return true;
            });

            //CheckForUnsubscribe(callbacksForProperty, propertyName);
        }

        private void ForEachCallback(string propertyName, Func<VersionedLinkedList<IWeakCallback>, IVersionedLinkedListNode<IWeakCallback>, bool> action)
        {
            var callbacksForProperty = LookupCallbacksForProperty(propertyName);
            if (callbacksForProperty == null)
            {
                return;
            }

            ulong version = callbacksForProperty.Version;
            var currentNode = callbacksForProperty.StartIterating(version);

            try
            {
                while (currentNode != null)
                {
                    if (!action(callbacksForProperty, currentNode))
                    {
                        break;
                    }

                    currentNode = callbacksForProperty.GetNext(version, currentNode);
                }
            }
            finally
            {
                callbacksForProperty.StopIterating();
            }
        }

        private void OnPropertyChanging(object sender, PropertyChangingEventArgs args)
        {
            //var callbacksForProperty = LookupCallbacksForProperty(args.PropertyName);
            //if (callbacksForProperty == null)
            //{
            //    return;
            //}

            //ulong version = callbacksForProperty.Version;
            //var currentNode = callbacksForProperty.StartIterating(version);

            //try
            //{
            //    while (currentNode != null)
            //    {
            //        IWeakEventCallback<PropertyChangingEventArgs> callback = currentNode.Value as IWeakEventCallback<PropertyChangingEventArgs>;
            //        if (callback != null)
            //        {
            //            if (!callback.Invoke(sender, args))
            //            {
            //                callbacksForProperty.MarkForRemove(currentNode);
            //            }
            //        }
            //        currentNode = callbacksForProperty.GetNext(version, currentNode);
            //    }
            //}
            //finally
            //{
            //    callbacksForProperty.StopIterating();
            //}
            ForEachCallback(args.PropertyName, (callbacksForProperty, currentNode) =>
            {
                IWeakEventCallback<PropertyChangingEventArgs> callback = currentNode.Value as IWeakEventCallback<PropertyChangingEventArgs>;
                if (callback != null)
                {
                    if (!callback.Invoke(sender, args))
                    {
                        callbacksForProperty.MarkForRemove(currentNode);
                    }
                }
                return true;
            });
        }

        private void CheckForUnsubscribe(VersionedLinkedList<IWeakCallback> callbacksForProperty, string propertyName)
        {
            if (callbacksForProperty.IsEmpty)
            {
                _propertyNameToCallbacks.Remove(propertyName);
            }

            UnsubscribeIfNoMoreListeners();
        }

        private void UnsubscribeIfNoMoreListeners()
        {
            if (!this.HasActiveListeners)
            {
                WeakPropertyChangedEventManager.UnregisterSource(_source);
                _source.PropertyChanged -= OnPropertyChanged;

                INotifyPropertyChanging sourceAsINotifyPropertyChanging = _source as INotifyPropertyChanging;
                if (sourceAsINotifyPropertyChanging != null)
                {
                    sourceAsINotifyPropertyChanging.PropertyChanging -= OnPropertyChanging;
                }
            }
        }

        public void AddListener<TListener>(
            string propertyName,
            TListener listener,
            Action<TListener, object, PropertyChangedEventArgs> propertyChangedCallback)
        {
            var callbacksForProperty = LookupOrCreateCallbacksForProperty(propertyName);
            var callback = new WeakEventCallback<TListener, PropertyChangedEventArgs>(listener, propertyChangedCallback);
            callbacksForProperty.AddLast(callback);
        }

        public void AddListener<TListener>(
            string propertyName,
            TListener listener,
            object rootSource,
            Action<TListener, object, object, PropertyChangingEventArgs> propertyChangingCallback,
            Action<TListener, object, object, PropertyChangedEventArgs> propertyChangedCallback)
        {
            var callbacksForProperty = LookupOrCreateCallbacksForProperty(propertyName);

            var callback = new WeakPropertyChangingCallback<TListener>(
                listener,
                rootSource,
                propertyChangingCallback,
                propertyChangedCallback);

            callbacksForProperty.AddLast(callback);
        }

        private VersionedLinkedList<IWeakCallback> LookupOrCreateCallbacksForProperty(string propertyName)
        {
            var callbacksForProperty = LookupCallbacksForProperty(propertyName);

            if (callbacksForProperty == null)
            {
                callbacksForProperty = new VersionedLinkedList<IWeakCallback>();
                _propertyNameToCallbacks.Add(propertyName, callbacksForProperty);
            }
            return callbacksForProperty;
        }

        private VersionedLinkedList<IWeakCallback> LookupCallbacksForProperty(string propertyName)
        {
            VersionedLinkedList<IWeakCallback> callbacksForProperty = null;

            if (_propertyNameToCallbacks.Contains(propertyName))
            {
                callbacksForProperty = (VersionedLinkedList<IWeakCallback>)_propertyNameToCallbacks[propertyName];
            }
            return callbacksForProperty;
        }
    }
}
