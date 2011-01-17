using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Collections.Specialized;

namespace ContinuousLinq.Aggregates
{
    public class OnlineCalculationContinuousValue<TSource, TColSelectorResult, TResult>
        : ContinuousValue<TSource, TColSelectorResult, TResult>
        where TSource : INotifyPropertyChanged
    {
        public Dictionary<TSource, TColSelectorResult> CurrentSelectorValues { get; set; }
        public Func<TColSelectorResult, TResult, TResult> ItemAddedValueUpdater { get; set; }
        public Func<TColSelectorResult, TResult, TResult> ItemRemovedValueUpdater { get; set; }
        public Func<TColSelectorResult, TColSelectorResult, TResult, TResult> ItemChangedValueUpdater { get; set; }

        public OnlineCalculationContinuousValue(
            IList<TSource> input,
            Expression<Func<TSource, TColSelectorResult>> selectorExpression,
            Func<TColSelectorResult, TResult, TResult> itemAddedValueUpdater,
            Func<TColSelectorResult, TResult, TResult> itemRemovedValueUpdater,
            Func<TColSelectorResult, TColSelectorResult, TResult, TResult> itemChangedValueUpdater,
            Action<TResult> afterEffect)
        {
            this.CurrentSelectorValues = new Dictionary<TSource, TColSelectorResult>(input.Count);
            this.ItemAddedValueUpdater = itemAddedValueUpdater;
            this.ItemChangedValueUpdater = itemChangedValueUpdater;
            this.ItemRemovedValueUpdater = itemRemovedValueUpdater;

            base.InitializeContinuousValue(input, selectorExpression, (source, selector) => OnRefresh());

            this.NotifyCollectionChangedMonitor.Add += OnItemAdded;
            this.NotifyCollectionChangedMonitor.Reset += OnReset;
            this.NotifyCollectionChangedMonitor.Replace += OnReplace;
            this.NotifyCollectionChangedMonitor.Remove += OnRemove;
            
            Refresh();
        }

        private TResult OnRefresh()
        {
            TResult newCurrentValue = default(TResult);

            foreach (var item in this.Source)
            {
                newCurrentValue = this.ItemAddedValueUpdater(this.Selector(item), newCurrentValue);
            }
            
            return newCurrentValue;
        }


        void OnItemAdded(object sender, int newStartingIndex, IEnumerable<TSource> newItems)
        {
            foreach (var item in newItems)
            {
                this.CurrentValue = this.ItemAddedValueUpdater(this.Selector(item), this.CurrentValue);
                var selectorResult = this.Selector(item);
                this.CurrentSelectorValues[item] = selectorResult;
            }
        }

        void OnRemove(object sender, int startingIndex, IEnumerable<TSource> items)
        {
            foreach (var item in items)
            {
                RemoveItem(item);
            }

            if (this.ItemRemovedValueUpdater == null)
            {
                Refresh();
            }
        }

        protected override void Refresh()
        {
            base.Refresh();

            if (!IsGloballyPaused)
            {
                CreateCurrentSelectorResults();
            }
        }

        private void RemoveItem(TSource item)
        {
            if (this.ItemRemovedValueUpdater != null)
            {
                this.CurrentValue = this.ItemRemovedValueUpdater(this.CurrentSelectorValues[item], this.CurrentValue);
            }

            if (!this.NotifyCollectionChangedMonitor.ReferenceCountTracker.Contains(item))
            {
                this.CurrentSelectorValues.Remove(item);
            }
        }

        void OnReplace(object sender, IEnumerable<TSource> oldItems, int newStartingIndex, IEnumerable<TSource> newItems)
        {
            foreach (var item in oldItems)
            {
                RemoveItem(item);
            }

            OnItemAdded(null, newStartingIndex, newItems);

            if (this.ItemRemovedValueUpdater == null)
            {
                Refresh();
            }
        }

        void OnReset(object sender)
        {
            Refresh();
        }

        private void CreateCurrentSelectorResults()
        {
            this.CurrentSelectorValues.Clear();

            foreach (TSource item in this.Source)
            {
                var selectorResult = this.Selector(item);
                this.CurrentSelectorValues[item] = selectorResult;
            }
        }

        protected override void OnItemChanged(object sender, INotifyPropertyChanged obj)
        {
            var item = (TSource)obj;

            TColSelectorResult currentSelectorValue = this.CurrentSelectorValues[item];
            TColSelectorResult newSelectorValue = this.Selector(item);

            int referenceCount = this.NotifyCollectionChangedMonitor.ReferenceCountTracker[item];
            for (int i = 0; i < referenceCount; i++)
            {
                this.CurrentValue = this.ItemChangedValueUpdater(currentSelectorValue, newSelectorValue, this.CurrentValue);
            }

            this.CurrentSelectorValues[item] = newSelectorValue;
        }

        protected override void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }
    }
}
