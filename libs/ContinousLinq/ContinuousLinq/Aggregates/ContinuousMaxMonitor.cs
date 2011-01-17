/*
 * CLINQ 
 * Continuous Maximum Monitor Class
 * Created by: Kevin Hoffman
 * Created on: 04/16/2008
 */
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace ContinuousLinq.Aggregates
{
    internal abstract class ContinuousMaxMonitor<Tinput, Toutput> : 
        AggregateViewAdapter<Tinput, Toutput> where Tinput : INotifyPropertyChanged
    {
        protected readonly Func<Tinput, Toutput> _maxFunc;

        protected ContinuousMaxMonitor(ObservableCollection<Tinput> input,
                                       Expression<Func<Tinput, Toutput>> maxFunc)
            : base(input,
            ExpressionPropertyAnalyzer.GetReferencedPropertyNames(maxFunc)[typeof(Tinput)],
            false)
        {
            _maxFunc = maxFunc.Compile();
            ReAggregate();
        }

        protected ContinuousMaxMonitor(ReadOnlyObservableCollection<Tinput> input,
                                       Expression<Func<Tinput, Toutput>> maxFunc)
            : base(input,
            ExpressionPropertyAnalyzer.GetReferencedPropertyNames(maxFunc)[typeof(Tinput)],
            false)
        {
            _maxFunc = maxFunc.Compile();
            ReAggregate();
        }
    }

    internal class ContinuousMaxMonitorInt<T> : ContinuousMaxMonitor<T, int> where T : INotifyPropertyChanged
    {
        public ContinuousMaxMonitorInt(ObservableCollection<T> input,
                                       Expression<Func<T, int>> maxFunc)
            : base(input, maxFunc)
        {
        }

        public ContinuousMaxMonitorInt(ReadOnlyObservableCollection<T> input,
                                       Expression<Func<T, int>> maxFunc)
            : base(input, maxFunc)
        {
        }

        protected override void ReAggregate()
        {
            if (this.Input.Count > 0)
            {
                SetCurrentValue(this.Input.Max(_maxFunc));
            }
            else
            {
                SetCurrentValueToDefault();
            }
        }
    }

    internal class ContinuousMaxMonitorLong<T> : ContinuousMaxMonitor<T, long> where T : INotifyPropertyChanged
    {
        public ContinuousMaxMonitorLong(ObservableCollection<T> input,
                                        Expression<Func<T, long>> maxFunc)
            : base(input, maxFunc)
        {
        }

        public ContinuousMaxMonitorLong(ReadOnlyObservableCollection<T> input,
                                        Expression<Func<T, long>> maxFunc)
            : base(input, maxFunc)
        {
        }

        protected override void ReAggregate()
        {
            if (this.Input.Count > 0)
            {
                SetCurrentValue(this.Input.Max(_maxFunc));
            }
            else
            {
                SetCurrentValueToDefault();
            }
        }
    }

    internal class ContinuousMaxMonitorDouble<T> : 
        ContinuousMaxMonitor<T, double> where T : INotifyPropertyChanged
    {
        public ContinuousMaxMonitorDouble(ObservableCollection<T> input,
                                          Expression<Func<T, double>> maxFunc)
            : base(input, maxFunc)
        {
        }

        public ContinuousMaxMonitorDouble(ReadOnlyObservableCollection<T> input,
                                          Expression<Func<T, double>> maxFunc)
            : base(input, maxFunc)
        {
        }

        protected override void ReAggregate()
        {
            if (this.Input.Count > 0)
            {
                SetCurrentValue(this.Input.Max(_maxFunc));
            }
            else
            {
                SetCurrentValueToDefault();
            }
        }
    }

    internal class ContinuousMaxMonitorFloat<T> : ContinuousMaxMonitor<T, float> where T : INotifyPropertyChanged
    {
        public ContinuousMaxMonitorFloat(ObservableCollection<T> input,
                                         Expression<Func<T, float>> maxFunc)
            : base(input, maxFunc)
        {
        }

        public ContinuousMaxMonitorFloat(ReadOnlyObservableCollection<T> input,
                                         Expression<Func<T, float>> maxFunc)
            : base(input, maxFunc)
        {
        }

        protected override void ReAggregate()
        {
            if (this.Input.Count > 0)
            {
                SetCurrentValue(this.Input.Max(_maxFunc));
            }
            else
            {
                SetCurrentValueToDefault();
            }
        }
    }

    internal class ContinuousMaxMonitorDecimal<T> : ContinuousMaxMonitor<T, decimal> where T : INotifyPropertyChanged
    {
        public ContinuousMaxMonitorDecimal(ObservableCollection<T> input,
                                           Expression<Func<T, decimal>> maxFunc)
            : base(input, maxFunc)
        {
        }

        public ContinuousMaxMonitorDecimal(ReadOnlyObservableCollection<T> input,
                                           Expression<Func<T, decimal>> maxFunc)
            : base(input, maxFunc)
        {
        }

        protected override void ReAggregate()
        {
            if (this.Input.Count > 0)
            {
                SetCurrentValue(this.Input.Max(_maxFunc));
            }
            else
            {
                SetCurrentValueToDefault();
            }
        }
    }
}