using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ContinuousLinq.Aggregates
{
    internal class ContinuousCountMonitor<T> : AggregateViewAdapter<T, int> where T : INotifyPropertyChanged
    {
        public ContinuousCountMonitor(ObservableCollection<T> input)
            : base(input, null, true)
        {
            ReAggregate();
        }

        public ContinuousCountMonitor(ReadOnlyObservableCollection<T> input)
            : base(input, null, true)
        {
            ReAggregate();
        }

        protected override void ReAggregate()
        {
            SetCurrentValue(this.Input.Count);
        }
    }
}