using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace ContinuousLinq
{  
    /// <summary>
    /// The GroupingViewAdapter is a view adapter that takes an input collection and applies
    /// a grouping algorithm to it (defined by the group-by clause in the original LINQ query).
    /// The results of the grouping clause are created in an output collection which we
    /// require to be a ContinuousCollection of GroupedContinuousCollection instances.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    ///  
    internal sealed class GroupingViewAdapter<TSource, TKey, TElement> :
        SmartViewAdapter<TSource, GroupedContinuousCollection<TKey, TElement>>
            where TSource : INotifyPropertyChanged
            where TKey : IComparable
            
    {
        private class GroupAndElement
        {
            public GroupedContinuousCollection<TKey, TElement> Group { get; private set; }
            public TElement Element { get; private set; }

            public GroupAndElement(GroupedContinuousCollection<TKey, TElement> group, TElement element)
            {
                this.Group = group;
                this.Element = element;
            }
        }

        private readonly Func<TSource, TKey> _keySelector;
        private readonly Func<TSource, TElement> _elementSelector;
        private readonly IEqualityComparer<TKey> _comparer;
        private readonly Dictionary<TKey, GroupedContinuousCollection<TKey, TElement>> _groupMap = 
            new Dictionary<TKey, GroupedContinuousCollection<TKey, TElement>>();
        // Maps an input item to its current output group.
        private readonly Dictionary<TSource, GroupAndElement> _outputShadowMap = 
            new Dictionary<TSource, GroupAndElement>();

        public GroupingViewAdapter(
            InputCollectionWrapper<TSource> source,
            LinqContinuousCollection<GroupedContinuousCollection<TKey, TElement>> output,
            Expression<Func<TSource, TKey>> keySelector,
            Expression<Func<TSource, TElement>> elementSelector,
            IEqualityComparer<TKey> comparer)        
                : base(source, output,
            ExpressionPropertyAnalyzer.GetReferencedPropertyNames(keySelector)[typeof(TSource)])
        {
            /*
             *     : base(source, output,
            ExpressionPropertyAnalyzer.GetReferencedPropertyNames(keySelector)[typeof(TSource)].
             * */

            
            if (elementSelector == null)
                throw new ArgumentNullException("elementSelector");
            _keySelector = keySelector.Compile();
            _elementSelector = elementSelector.Compile();
            if (comparer == null)
            {
                comparer = EqualityComparer<TKey>.Default;
            }
            _comparer = comparer;

            foreach (TSource item in this.InputCollection)
            {
                AddItem(item, -1);
            }
        }

        private void AddItem(TSource item, int dummy)
        {
            AddItem(item, _elementSelector(item));
        }

        private void AddItem(TSource item, TElement element)
        {
            TKey theKey = _keySelector(item);
            GroupedContinuousCollection<TKey, TElement> outputGroup;
            if (_groupMap.TryGetValue(theKey, out outputGroup) == false)
            {
                outputGroup = new GroupedContinuousCollection<TKey, TElement>(theKey, _comparer);
                this.OutputCollection.Add(outputGroup);
                _groupMap[theKey] = outputGroup;
            }
            try
            {
                outputGroup.IsSealed = false;
                outputGroup.Add(element);
            }
            finally
            {
                outputGroup.IsSealed = true;
            }
            _outputShadowMap[item] = new GroupAndElement(outputGroup, element);
        }

        private void MaybeMoveItem(TSource item, GroupAndElement info)
        {
            TKey newKey = _keySelector(item);
            TElement newElement = _elementSelector(item);
            if (info.Group.Key.Equals(newKey) == false || info.Element.Equals(newElement) == false)
            {
                RemoveItem(item);
                AddItem(item, newElement);
            }
        }

        private bool RemoveItem(TSource item)
        {
            GroupAndElement info;
            if (_outputShadowMap.TryGetValue(item, out info))
            {
                _outputShadowMap.Remove(item);
                GroupedContinuousCollection<TKey, TElement> group = info.Group;
                bool wasRemoved;
                try
                {
                    group.IsSealed = false;
                    wasRemoved = group.Remove(info.Element);
                }
                finally
                {
                    group.IsSealed = true;
                }
                if (group.Count == 0)
                {
                    this.OutputCollection.Remove(group);
                    _groupMap.Remove(group.Key);
                }
                return wasRemoved;
            }
            else
                return false;
        }

        #region SmartViewAdapter overrides
        protected override void ItemAdded(TSource item)
        {
            AddItem(item, _elementSelector(item));
        }
        protected override bool ItemRemoved(TSource item)
        {
            return RemoveItem(item);
        }
        protected override void ItemsCleared()
        {
            this.OutputCollection.Clear();
            _groupMap.Clear();
            _outputShadowMap.Clear();
        }
        protected override void ItemPropertyChanged(TSource item)
        {
            GroupAndElement info;
            if (_outputShadowMap.TryGetValue(item, out info))
                MaybeMoveItem(item, info);
        }
        #endregion
        
        public override void ReEvaluate()
        {
            foreach (KeyValuePair<TSource, GroupAndElement> pair in _outputShadowMap)
            {
                TSource item = pair.Key;
                GroupAndElement info = pair.Value;
                MaybeMoveItem(item, info);
            }
        }
    }
}
