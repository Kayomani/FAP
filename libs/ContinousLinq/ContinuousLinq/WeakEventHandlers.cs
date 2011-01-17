using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ContinuousLinq
{
    /// <summary>
    /// Specialized version of weak event handlers so that when output result sets go out of scope
    /// and are collected, the chain of handlers/listeners can also be collected and disposed. These
    /// are necessary so that we can adequately manage the memory used by the ViewAdapter chain when
    /// either end goes out of scope and is collected.
    /// Thanks to Erb Cooper for this implementation.
    /// </summary>
    internal class WeakCollectionChangedHandler : WeakReference
    {
        private readonly INotifyCollectionChanged _owner;

        public WeakCollectionChangedHandler(INotifyCollectionChanged owner, 
            NotifyCollectionChangedEventHandler target) : base(target)
        {
            _owner = owner;
            _owner.CollectionChanged += OnCollectionChanged;
        }

        public void Detach()
        {
            this.Target = null;
            _owner.CollectionChanged -= OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {            
            NotifyCollectionChangedEventHandler target = (NotifyCollectionChangedEventHandler)this.Target;
            if (target != null)
            {              
                target.Invoke(sender, args);
            }
            else
            {
                _owner.CollectionChanged -= OnCollectionChanged;
            }
        }
    }

    internal class WeakPropertyChangedHandler : WeakReference
    {
        private readonly INotifyPropertyChanged _owner;

        public WeakPropertyChangedHandler(INotifyPropertyChanged owner, PropertyChangedEventHandler target)
            : base(target)
        {
            _owner = owner;
            _owner.PropertyChanged += OnPropertyChanged;
        }

        public void Detach()
        {
            this.Target = null;
            _owner.PropertyChanged -= OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            PropertyChangedEventHandler target = (PropertyChangedEventHandler)this.Target;
            if (target != null)
            {
                target.Invoke(sender, args);
            }
            else
            {
                _owner.PropertyChanged -= OnPropertyChanged;
            }
        }
    }
}
