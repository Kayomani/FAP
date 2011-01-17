using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace ContinuousLinq
{   
    internal sealed class DistinctViewAdapter<TSource> : ViewAdapter<TSource, TSource>
        where TSource : INotifyPropertyChanged
    {
        private IEqualityComparer<TSource> _comparer = null;

        public DistinctViewAdapter(InputCollectionWrapper<TSource> source,
            LinqContinuousCollection<TSource> output) : base(source, output)
        {
            ReEvaluate();
        }

        public DistinctViewAdapter(InputCollectionWrapper<TSource> source,
            LinqContinuousCollection<TSource> output,
            IEqualityComparer<TSource> comparer)
            : base(source, output)
        {
            _comparer = comparer;
            ReEvaluate();
        }

        protected override void AddItem(TSource newItem, int index)
        {            
            ReEvaluate();
        }

        protected override void Clear()
        {
            this.OutputCollection.Clear();
        }

        protected override void OnCollectionItemPropertyChanged(TSource item, string propertyName)
        {
            ReEvaluate();
        }

        protected override void OnInputCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnInputCollectionChanged(e);
            ReEvaluate();
        }

        protected override bool RemoveItem(TSource newItem, int index)
        {
            return this.OutputCollection.Remove(newItem);
        }                  

        public override void ReEvaluate()
        {            
            this.OutputCollection.Clear();
            IEnumerable<TSource> distinctItems;

            if (_comparer != null)
                distinctItems = InputCollection.Distinct<TSource>(_comparer);
            else
                distinctItems = InputCollection.Distinct<TSource>();

            this.OutputCollection.AddRange(distinctItems);
        }        
    }
}
