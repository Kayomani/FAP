using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace ContinuousLinq.Aggregates
{
    internal abstract class ContinuousStdDevMonitor<Tinput, Tfunc> : AggregateViewAdapter<Tinput, double> where Tinput : INotifyPropertyChanged
    {
        protected readonly Func<Tinput, Tfunc> _selector;

        protected ContinuousStdDevMonitor(ObservableCollection<Tinput> input,
                                          Expression<Func<Tinput, Tfunc>> devValueSelector)
            : base(input,
            ExpressionPropertyAnalyzer.GetReferencedPropertyNames(devValueSelector)[typeof(Tinput)],
            false)
        {
            _selector = devValueSelector.Compile();
            ReAggregate();
        }

        protected ContinuousStdDevMonitor(ReadOnlyObservableCollection<Tinput> input,
                                          Expression<Func<Tinput, Tfunc>> devValueSelector)
            : base(input,
            ExpressionPropertyAnalyzer.GetReferencedPropertyNames(devValueSelector)[typeof(Tinput)],
            false)
        {
            _selector = devValueSelector.Compile();
            ReAggregate();
        }
    }

    internal class ContinuousStdDevMonitorInt<T> : ContinuousStdDevMonitor<T, int> where T : INotifyPropertyChanged
    {
        public ContinuousStdDevMonitorInt(ObservableCollection<T> input,
                                          Expression<Func<T, int>> devValueSelector)
            : base(input, devValueSelector)
        {
        }

        public ContinuousStdDevMonitorInt(ReadOnlyObservableCollection<T> input,
                                          Expression<Func<T, int>> devValueSelector)
            : base(input, devValueSelector)
        {
        }

        protected override void ReAggregate()
        {
            if (this.Input.Count > 0)
            {
                SetCurrentValue(StdDev.Compute(_selector, this.Input));
            }
            else
            {
                SetCurrentValueToDefault();
            }
        }
    }

    internal class ContinuousStdDevMonitorDouble<T> : ContinuousStdDevMonitor<T, double> where T : INotifyPropertyChanged
    {
        public ContinuousStdDevMonitorDouble(ObservableCollection<T> input,
                                             Expression<Func<T, double>> devValueSelector)
            : base(input, devValueSelector)
        {
        }

        public ContinuousStdDevMonitorDouble(ReadOnlyObservableCollection<T> input,
                                             Expression<Func<T, double>> devValueSelector)
            : base(input, devValueSelector)
        {
        }

        protected override void ReAggregate()
        {
            if (this.Input.Count > 0)
            {
                SetCurrentValue(StdDev.Compute(_selector, this.Input));
            }
            else
            {
                SetCurrentValueToDefault();
            }
        }
    }

    internal class ContinuousStdDevMonitorFloat<T> : ContinuousStdDevMonitor<T, float> where T : INotifyPropertyChanged
    {
        public ContinuousStdDevMonitorFloat(ObservableCollection<T> input,
                                            Expression<Func<T, float>> devValueSelector)
            : base(input, devValueSelector)
        {
        }

        public ContinuousStdDevMonitorFloat(ReadOnlyObservableCollection<T> input,
                                            Expression<Func<T, float>> devValueSelector)
            : base(input, devValueSelector)
        {
        }

        protected override void ReAggregate()
        {
            if (this.Input.Count > 0)
            {
                SetCurrentValue(StdDev.Compute(_selector, this.Input));
            }
            else
            {
                SetCurrentValueToDefault();
            }
        }
    }

    internal class ContinuousStdDevMonitorDecimal<T> : ContinuousStdDevMonitor<T, decimal> where T : INotifyPropertyChanged
    {
        public ContinuousStdDevMonitorDecimal(ObservableCollection<T> input,
                                              Expression<Func<T, decimal>> devValueSelector)
            : base(input, devValueSelector)
        {
        }

        public ContinuousStdDevMonitorDecimal(ReadOnlyObservableCollection<T> input,
                                              Expression<Func<T, decimal>> devValueSelector)
            : base(input, devValueSelector)
        {
        }

        protected override void ReAggregate()
        {
            if (this.Input.Count > 0)
            {
                SetCurrentValue(StdDev.Compute(_selector, this.Input));
            }
            else
            {
                SetCurrentValueToDefault();
            }
        }
    }

    internal class ContinuousStdDevMonitorLong<T> : ContinuousStdDevMonitor<T, long> where T : INotifyPropertyChanged
    {
        public ContinuousStdDevMonitorLong(ObservableCollection<T> input,
                                           Expression<Func<T, long>> devValueSelector)
            : base(input, devValueSelector)
        {
        }

        public ContinuousStdDevMonitorLong(ReadOnlyObservableCollection<T> input,
                                           Expression<Func<T, long>> devValueSelector)
            : base(input, devValueSelector)
        {
        }

        protected override void ReAggregate()
        {
            if (this.Input.Count > 0)
            {
                SetCurrentValue(StdDev.Compute(_selector, this.Input));
            }
            else
            {
                SetCurrentValueToDefault();
            }
        }
    }
}