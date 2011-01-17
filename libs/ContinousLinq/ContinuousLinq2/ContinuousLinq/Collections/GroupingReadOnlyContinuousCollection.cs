using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using ContinuousLinq.Expressions;
using ContinuousLinq;

namespace ContinuousLinq.Collections
{
    public class GroupingReadOnlyContinuousCollection<TKey,TSource> 
        : ReadOnlyAdapterContinuousCollection<TSource, GroupedReadOnlyContinuousCollection<TKey, TSource>>
    {
        internal ContinuousCollection<GroupedReadOnlyContinuousCollection<TKey, TSource>> Output { get; set; }

        internal Func<TSource, TKey> KeySelector { get; set; }

        internal Dictionary<TSource, GroupedReadOnlyContinuousCollection<TKey, TSource>> ItemToGroupIndex { get; set; }

        private ReadOnlyContinuousCollection<TKey> _keys;

        public ReadOnlyContinuousCollection<TKey> Keys
        {
            get
            {
                if (_keys == null)
                {
                    _keys = from col in this.Output
                            select col.Key;
                }
                return _keys;
            }
        }

        internal GroupingReadOnlyContinuousCollection(
            IList<TSource> list,
            Expression<Func<TSource, TKey>> keySelectorExpression) 
            : base(list, ExpressionPropertyAnalyzer.Analyze(keySelectorExpression))
        {
            this.KeySelector = keySelectorExpression.CachedCompile();
            this.Output = new ContinuousCollection<GroupedReadOnlyContinuousCollection<TKey, TSource>>();

            this.ItemToGroupIndex = new Dictionary<TSource, GroupedReadOnlyContinuousCollection<TKey, TSource>>();

            AddNewItems(this.Source);

            this.NotifyCollectionChangedMonitor.Add += OnAdd;
            this.NotifyCollectionChangedMonitor.Remove += OnRemove;
            this.NotifyCollectionChangedMonitor.Reset += OnReset;
            this.NotifyCollectionChangedMonitor.Replace += OnReplace;
            this.NotifyCollectionChangedMonitor.ItemChanged += OnItemChanged;

            this.Output.CollectionChanged += OnOutputCollectionChanged;
        }

        private void OnOutputCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            RefireCollectionChanged(args);
        }

        private GroupedReadOnlyContinuousCollection<TKey, TSource> GetCollectionForKey(TKey key)
        {
            GroupedReadOnlyContinuousCollection<TKey, TSource> group =
                this.Output.FirstOrDefault(item => EqualityComparer<TKey>.Default.Equals(item.Key, key));
            
            if (group == null)
            {
                group = new GroupedReadOnlyContinuousCollection<TKey, TSource>(key);
                this.Output.Add(group);
            }
            return group;
        }

        private void AddNewItems(IEnumerable<TSource> source)
        {
            foreach (TSource item in source)
            {
                GroupedReadOnlyContinuousCollection<TKey, TSource> group;
                if(!this.ItemToGroupIndex.TryGetValue(item, out group))
                {
                    group = GetCollectionForKey(this.KeySelector(item));
                    this.ItemToGroupIndex[item] = group;
                }

                group.InternalCollection.Add(item);
            }            
        }

        private void RemoveOldItems(IEnumerable<TSource> oldItems)
        {
            foreach (TSource item in oldItems)
            {
                var currentGroup = this.ItemToGroupIndex[item];
                currentGroup.InternalCollection.Remove(item);

                if (currentGroup.Count == 0)
                {
                    this.Output.Remove(currentGroup);
                }

                if (!this.NotifyCollectionChangedMonitor.ReferenceCountTracker.Contains(item))
                {
                    this.ItemToGroupIndex.Remove(item);
                }
            }
        }

        private void Regroup(TSource modifiedItem)
        {
            var currentGroup = this.ItemToGroupIndex[modifiedItem];
            
            TKey newKey = this.KeySelector(modifiedItem);

            if (EqualityComparer<TKey>.Default.Equals(currentGroup.Key, newKey))
            {
                return;
            }

            GroupedReadOnlyContinuousCollection<TKey, TSource> newGroupForModifiedItem = GetCollectionForKey(newKey);

            while (currentGroup.InternalCollection.Remove(modifiedItem)) 
            {
                newGroupForModifiedItem.InternalCollection.Add(modifiedItem);
            }

            if (currentGroup.Count == 0)
            {
                this.Output.Remove(currentGroup);
            }

            this.ItemToGroupIndex[modifiedItem] = newGroupForModifiedItem;
        }

        #region Source Changed Event Handlers

        void OnAdd(object sender, int index, IEnumerable<TSource> newItems)
        {
            AddNewItems(newItems);
        }

        void OnItemChanged(object sender, INotifyPropertyChanged itemThatChanged)
        {
            TSource itemThatChangedAsSource = (TSource)itemThatChanged;
            Regroup(itemThatChangedAsSource);
        }

        void OnRemove(object sender, int index, IEnumerable<TSource> oldItems)
        {
            RemoveOldItems(oldItems);
        }

        void OnReset(object sender)
        {
            this.Output.Clear();
            this.ItemToGroupIndex.Clear();
        }

        void OnReplace(object sender, IEnumerable<TSource> oldItems, int newStartingIndex, IEnumerable<TSource> newItems)
        {
            RemoveOldItems(oldItems);
            AddNewItems(newItems);
        }

        #endregion

        #region Overrides
        
        public override int Count
        {
            get { return Output.Count; }
        }

        public override GroupedReadOnlyContinuousCollection<TKey, TSource> this[int index]
        {
            get { return this.Output[index]; }
            set { throw new NotImplementedException(); }
        }
        
        #endregion


    }
}
