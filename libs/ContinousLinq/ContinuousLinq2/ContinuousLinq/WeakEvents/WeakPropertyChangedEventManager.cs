using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace ContinuousLinq.WeakEvents
{
    public static class WeakPropertyChangedEventManager
    {
        internal static WeakDictionary<INotifyPropertyChanged, WeakPropertyBridge> SourceToBridgeTable { get; private set; }

        static WeakPropertyChangedEventManager()
        {
            SourceToBridgeTable = new WeakDictionary<INotifyPropertyChanged, WeakPropertyBridge>();
        }

        public static void Register<TListener>(
            INotifyPropertyChanged source,
            string propertyName,
            TListener listener,
            Action<TListener, object, PropertyChangedEventArgs> propertyChangedCallback)
        {
            WeakPropertyBridge bridge;
            bridge = GetBridgeForSource(source);

            bridge.AddListener(propertyName, listener, propertyChangedCallback);
        }

        public static void Register<TListener>(
            INotifyPropertyChanged source,
            object rootSource, 
            string propertyName,
            TListener listener,
            Action<TListener, object, object, PropertyChangingEventArgs> propertyChangingCallback,
            Action<TListener, object, object, PropertyChangedEventArgs> propertyChangedCallback)
        {
            WeakPropertyBridge bridge;
            bridge = GetBridgeForSource(source);

            bridge.AddListener(
                propertyName, 
                listener, 
                rootSource,
                propertyChangingCallback,
                propertyChangedCallback);
        }

        private static WeakPropertyBridge GetBridgeForSource(INotifyPropertyChanged source)
        {
            WeakPropertyBridge bridge;

            if (!SourceToBridgeTable.TryGetValue(source, out bridge))
            {
                bridge = AddNewPropertyBridgeToTable(source);
            }

            //Can happen if the GC does it's magic
            if (bridge == null)
            {
                bridge = AddNewPropertyBridgeToTable(source);
            }
            return bridge;
        }

        private static WeakPropertyBridge AddNewPropertyBridgeToTable(
            INotifyPropertyChanged source)
        {
            WeakPropertyBridge bridge = new WeakPropertyBridge(source);
            SourceToBridgeTable[source] = bridge;
            return bridge;
        }

        private static void OnAllListenersForSourceUnsubscribed(INotifyPropertyChanged source)
        {
            UnregisterSource(source);
        }

        public static void Unregister(INotifyPropertyChanged source, string propertyName, object listener, object rootSubject)
        {
            WeakPropertyBridge bridge;

            if (!SourceToBridgeTable.TryGetValue(source, out bridge))
            {
                return;
            }

            if (bridge == null)
            {
                SourceToBridgeTable.Remove(source);
                return;
            }

            bridge.RemoveListener(listener, propertyName, rootSubject);
        }

        public static void UnregisterSource(INotifyPropertyChanged source)
        {
            SourceToBridgeTable.Remove(source);
        }

        public static void RemoveCollectedEntries()
        {
            SourceToBridgeTable.RemoveCollectedEntries();
        }
    }
}
