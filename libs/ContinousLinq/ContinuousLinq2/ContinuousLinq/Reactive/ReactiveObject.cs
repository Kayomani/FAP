using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;

namespace ContinuousLinq.Reactive
{
    public interface IReactiveObject : INotifyPropertyChanged, INotifyPropertyChanging
    {
    }

    public abstract class ReactiveObject : IReactiveObject
    {
        #region Events & Delegates

        public event PropertyChangedEventHandler PropertyChanged;
        
        public event PropertyChangingEventHandler PropertyChanging;
        
        #endregion

        #region Constructors

        static ReactiveObject()
        {
            DependsOn = new Dictionary<Type, IDependsOn>();
        }

        protected ReactiveObject()
        {
#if !SILVERLIGHT
            this.Dispatcher = Dispatcher.CurrentDispatcher;
#endif
            Type type = this.GetType();

            CreateSubscriptionsStartingBaseFirst(type);
        }

        #endregion

        #region Properties

        private static Dictionary<Type, IDependsOn> DependsOn { get; set; }

        private List<SubscriptionTree> _subscriptionTrees;

        public bool SuppressPropertyChanged { get; set; }

        public Dispatcher Dispatcher { get; set; }

        #endregion

        #region Methods

        private void CreateSubscriptionsStartingBaseFirst(Type type)
        {
            if (type == typeof(ReactiveObject))
                return;

            CreateSubscriptionsStartingBaseFirst(type.BaseType);

            IDependsOn dependsOn;
            if (DependsOn.TryGetValue(type, out dependsOn))
            {
                dependsOn.CreateSubscriptions(this, ref _subscriptionTrees);
            }
        }

        protected static DependsOn<T> Register<T>() where T : class, INotifyPropertyChanged
        {
            Type type = typeof(T);

            if (DependsOn.ContainsKey(type))
                throw new InvalidOperationException("Type has already been registered for: " + typeof(T));

            var dependsOn = new DependsOn<T>();

            DependsOn[type] = dependsOn;

            return dependsOn;
        }

        [DebuggerNonUserCode]
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null || this.SuppressPropertyChanged)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        [DebuggerNonUserCode]
        protected void OnPropertyChanging(string propertyName)
        {
            if (PropertyChanging == null || this.SuppressPropertyChanged)
                return;

            PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }

        #endregion
    }
}