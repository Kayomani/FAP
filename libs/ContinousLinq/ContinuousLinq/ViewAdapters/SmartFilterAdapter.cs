using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Diagnostics;

namespace ContinuousLinq
{
    sealed internal class SmartFilterAdapter<T> : SmartViewAdapter<T,T>
        where T : INotifyPropertyChanged
    {
        private Func<T, bool> _predicate;

        public SmartFilterAdapter(
            InputCollectionWrapper<T> input,
            LinqContinuousCollection<T> output,
            Expression<Func<T,bool>> predicateExpr) :
                base(input,
                     output, 
                    ExpressionPropertyAnalyzer.GetReferencedPropertyNames(predicateExpr)[typeof(T)])
        {
            _predicate = predicateExpr.Compile();

            foreach (T item in input.InnerAsList)
            {
                if (_predicate(item))
                    OutputCollection.Add(item);
            }
        }

        /// <summary>
        /// This is ONLY called when the developer needs to manually re-evaluate the
        /// entire list...which should, in theory, never need to be done.
        /// </summary>
        public override void ReEvaluate()
        {
            FilterAll();
        }

        protected override void ItemPropertyChanged(T item)
        {
            Debug.WriteLine("[SFA] Property changed on item.");
            Filter(item);
        }


        protected override void ItemAdded(T item)
        {
            if ( (_predicate != null) &&
                (_predicate(item)))
                this.OutputCollection.Add(item);
        }

        protected override bool ItemRemoved(T item)
        {
            return this.OutputCollection.Remove(item);
        }

        protected override void ItemsCleared()
        {
            this.OutputCollection.Clear();
        }

        private void FilterAll()
        {
            foreach (T item in InputCollection)
                Filter(item);
        }
        
        private void Filter(T item)
        {            
            if (_predicate != null)
            {
                if (!_predicate(item))
                {
                    this.OutputCollection.Remove(item);
                }
                else if (!this.OutputCollection.Contains(item))
                {
                    this.OutputCollection.Add(item);
                }
            }
        }                            
    }
}
