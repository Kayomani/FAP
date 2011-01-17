using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ContinuousLinq
{
    /// <summary>
    /// This class provides us with a bit of tricky-work that allows us to create continuous queries
    /// on ObservableCollections and ReadOnlyObservableCollections as well as ContinousCollections. This
    /// lets these queries work on the 3 data types without having to re-write the CQ extension 3 different times,
    /// preserving the DRY principal as much as possible.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class InputCollectionWrapper<T>
    {
        private readonly IList<T> _inner;
        private readonly LinqContinuousCollection<T> _innerAsCC;

        public InputCollectionWrapper(ObservableCollection<T> inner)
        {
            _inner = inner;
            _innerAsCC = inner as LinqContinuousCollection<T>;
        }

        public InputCollectionWrapper(ReadOnlyObservableCollection<T> inner)
        {
            _inner = inner;
        }

        public IViewAdapter SourceAdapter
        {
            get { return (_innerAsCC != null ? _innerAsCC.SourceAdapter : null); }
        }

        public INotifyCollectionChanged InnerAsNotifier
        {
             get { return (INotifyCollectionChanged)_inner; }
        }

        public IList<T> InnerAsList
        {
             get { return _inner; }
        }
    }
}
