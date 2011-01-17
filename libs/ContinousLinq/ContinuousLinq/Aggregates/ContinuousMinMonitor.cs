/*
 * CLINQ 
 * Continuous Minimum Monitor Class
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
    internal abstract class ContinuousMinMonitor<Tinput, Toutput> : AggregateViewAdapter<Tinput, Toutput> where Tinput : INotifyPropertyChanged
    {
        protected readonly Func<Tinput, Toutput> _minFunc;

        protected ContinuousMinMonitor(ObservableCollection<Tinput> input,
                                       Expression<Func<Tinput, Toutput>> minFunc)
            : base(input,
            ExpressionPropertyAnalyzer.GetReferencedPropertyNames(minFunc)[typeof(Tinput)],
            false)            
        {
            _minFunc = minFunc.Compile();
            ReAggregate();
        }

        protected ContinuousMinMonitor(ReadOnlyObservableCollection<Tinput> input,
                                       Expression<Func<Tinput, Toutput>> minFunc)
            : base(input,
            ExpressionPropertyAnalyzer.GetReferencedPropertyNames(minFunc)[typeof(Tinput)],
            false)
        {
            _minFunc = minFunc.Compile();
            ReAggregate();
        }
    }

    internal class ContinuousMinMonitorInt<T> : ContinuousMinMonitor<T, int> where T : INotifyPropertyChanged
    {
        public ContinuousMinMonitorInt(ObservableCollection<T> input,
                                       Expression<Func<T, int>> minFunc)
            : base(input, minFunc)
        {
        }

        public ContinuousMinMonitorInt(ReadOnlyObservableCollection<T> input,
                                       Expression<Func<T, int>> minFunc)
            : base(input, minFunc)
        {
        }

        protected override void ReAggregate()
        {
            if (this.Input.Count > 0)
            {
                SetCurrentValue(this.Input.Min(_minFunc));
            }
            else
            {
                SetCurrentValueToDefault();
            }
        }
    }

    internal class ContinuousMinMonitorLong<T> : ContinuousMinMonitor<T, long> where T : INotifyPropertyChanged
    {
        public ContinuousMinMonitorLong(ObservableCollection<T> input,
                                        Expression<Func<T, long>> minFunc)
            : base(input, minFunc)
        {
        }

        public ContinuousMinMonitorLong(ReadOnlyObservableCollection<T> input,
                                        Expression<Func<T, long>> minFunc)
            : base(input, minFunc)
        {
        }

        protected override void ReAggregate()
        {
            if (this.Input.Count > 0)
            {
                SetCurrentValue(this.Input.Min(_minFunc));
            }
            else
            {
                SetCurrentValueToDefault();
            }
        }
    }

    internal class ContinuousMinMonitorDouble<T> : ContinuousMinMonitor<T, double> where T : INotifyPropertyChanged
    {
        public ContinuousMinMonitorDouble(ObservableCollection<T> input,
                                          Expression<Func<T, double>> minFunc)
            : base(input, minFunc)
        {
        }

        public ContinuousMinMonitorDouble(ReadOnlyObservableCollection<T> input,
                                          Expression<Func<T, double>> minFunc)
            : base(input, minFunc)
        {
        }

        protected override void ReAggregate()
        {
            if (this.Input.Count > 0)
            {
                SetCurrentValue(this.Input.Min(_minFunc));
            }
            else
            {
                SetCurrentValueToDefault();
            }
        }
    }

    internal class ContinuousMinMonitorFloat<T> : ContinuousMinMonitor<T, float> where T : INotifyPropertyChanged
    {
        public ContinuousMinMonitorFloat(ObservableCollection<T> input,
                                         Expression<Func<T, float>> minFunc)
            : base(input, minFunc)
        {
        }

        public ContinuousMinMonitorFloat(ReadOnlyObservableCollection<T> input,
                                         Expression<Func<T, float>> minFunc)
            : base(input, minFunc)
        {
        }

        protected override void ReAggregate()
        {
            if (this.Input.Count > 0)
            {
                SetCurrentValue(this.Input.Min(_minFunc));
            }
            else
            {
                SetCurrentValueToDefault();
            }
        }
    }

    internal class ContinuousMinMonitorDecimal<T> : ContinuousMinMonitor<T, decimal> where T : INotifyPropertyChanged
    {
        public ContinuousMinMonitorDecimal(ObservableCollection<T> input,
                                           Expression<Func<T, decimal>> minFunc)
            : base(input, minFunc)
        {
        }

        public ContinuousMinMonitorDecimal(ReadOnlyObservableCollection<T> input,
                                           Expression<Func<T, decimal>> minFunc)
            : base(input, minFunc)
        {
        }

        protected override void ReAggregate()
        {
            if (this.Input.Count > 0)
            {
                SetCurrentValue(this.Input.Min(_minFunc));
            }
            else
            {
                SetCurrentValueToDefault();
            }
        }
    }
}