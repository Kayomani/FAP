/*
 * CLINQ 
 * Continuous average Monitor Class
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
    internal abstract class ContinuousAverageMonitor<Tinput, Tfunc, Toutput> : AggregateViewAdapter<Tinput, Toutput>
        where Tinput : INotifyPropertyChanged
    {
        protected readonly Func<Tinput, Tfunc> _averageFunc;

        protected ContinuousAverageMonitor(ObservableCollection<Tinput> input,
                                           Expression<Func<Tinput, Tfunc>> averageFunc)
            : base(input,
            ExpressionPropertyAnalyzer.GetReferencedPropertyNames(averageFunc)[typeof(Tinput)],
            false)
        {
            _averageFunc = averageFunc.Compile();
            ReAggregate();
        }

        protected ContinuousAverageMonitor(ReadOnlyObservableCollection<Tinput> input,
                                           Expression<Func<Tinput, Tfunc>> averageFunc)
            : base(input,
            ExpressionPropertyAnalyzer.GetReferencedPropertyNames(averageFunc)[typeof(Tinput)],
            false)
        {
            _averageFunc = averageFunc.Compile();
            ReAggregate();
        }
    }

    internal class ContinuousAverageMonitorDecimal<T> : ContinuousAverageMonitor<T, decimal, decimal>
        where T : INotifyPropertyChanged
    {
        public ContinuousAverageMonitorDecimal(ObservableCollection<T> input,
                                               Expression<Func<T, decimal>> averageFunc)
            : base(input, averageFunc)
        {
        }

        public ContinuousAverageMonitorDecimal(ReadOnlyObservableCollection<T> input,
                                               Expression<Func<T, decimal>> averageFunc)
            : base(input, averageFunc)
        {
        }

        protected override void ReAggregate()
        {
            if (this.Input.Count > 0)
            {
                SetCurrentValue(this.Input.Average(_averageFunc));
            }
            else
            {
                SetCurrentValueToDefault();
            }
        }
    }

    internal class ContinuousAverageMonitorDouble<T> : ContinuousAverageMonitor<T, double, double >
        where T : INotifyPropertyChanged
    {
        public ContinuousAverageMonitorDouble(ObservableCollection<T> input,
                                              Expression<Func<T, double>> averageFunc)
            : base(input, averageFunc)
        {
        }

        public ContinuousAverageMonitorDouble(ReadOnlyObservableCollection<T> input,
                                              Expression<Func<T, double>> averageFunc)
            : base(input, averageFunc)
        {
        }

        protected override void ReAggregate()
        {
            if (this.Input.Count > 0)
            {
                SetCurrentValue(this.Input.Average(_averageFunc));
            }
            else
            {
                SetCurrentValueToDefault();
            }
        }
    }

    internal class ContinuousAverageMonitorFloat<T> : ContinuousAverageMonitor<T, float, float>
        where T : INotifyPropertyChanged
    {
        public ContinuousAverageMonitorFloat(ObservableCollection<T> input,
                                             Expression<Func<T, float>> averageFunc)
            : base(input, averageFunc)
        {
        }

        public ContinuousAverageMonitorFloat(ReadOnlyObservableCollection<T> input,
                                             Expression<Func<T, float>> averageFunc)
            : base(input, averageFunc)
        {
        }

        protected override void ReAggregate()
        {
            if (this.Input.Count > 0)
            {
                SetCurrentValue(this.Input.Average(_averageFunc));
            }
            else
            {
                SetCurrentValueToDefault();
            }
        }
    }

    internal class ContinuousAverageMonitorDoubleInt<T> : ContinuousAverageMonitor<T, int, double> where T : INotifyPropertyChanged
    {
        public ContinuousAverageMonitorDoubleInt(ObservableCollection<T> input,
                                                 Expression<Func<T, int>> averageFunc)
            : base(input, averageFunc)
        {
        }

        public ContinuousAverageMonitorDoubleInt(ReadOnlyObservableCollection<T> input,
                                                 Expression<Func<T, int>> averageFunc)
            : base(input, averageFunc)
        {
        }

        protected override void ReAggregate()
        {
            if (this.Input.Count > 0)
            {
                SetCurrentValue(this.Input.Average(_averageFunc));
            }
            else
            {
                SetCurrentValueToDefault();
            }
        }
    }

    internal class ContinuousAverageMonitorDoubleLong<T> : ContinuousAverageMonitor<T, long, double> where T : INotifyPropertyChanged
    {
        public ContinuousAverageMonitorDoubleLong(ObservableCollection<T> input,
                                                  Expression<Func<T, long>> averageFunc)
            : base(input, averageFunc)
        {
        }

        public ContinuousAverageMonitorDoubleLong(ReadOnlyObservableCollection<T> input,
                                                  Expression<Func<T, long>> averageFunc)
            : base(input, averageFunc)
        {
        }

        protected override void ReAggregate()
        {
            if (this.Input.Count > 0)
            {
                SetCurrentValue(this.Input.Average(_averageFunc));
            }
            else
            {
                SetCurrentValueToDefault();
            }
        }
    }
}