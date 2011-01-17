using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Linq.Expressions;

namespace ContinuousLinq.Reactive
{
    public class DependsOnMethod<T> where T : class, INotifyPropertyChanged
    {
        private Action<T> _callback;
        private List<PropertyAccessTree> _accessTreesWithoutNotifyPropertyChanging;
        private List<IPropertyAccessTreeSubscriber<DependsOnMethod<T>>> _propertyChangeSubscribers;

        #region Constructor

        internal DependsOnMethod(Action<T> callback)
        {
            _callback = callback;
        }

        #endregion

        #region Methods

        public DependsOnMethod<T> OnChanged<TResult>(Expression<Func<T, TResult>> propertyAccessor)
        {
            PropertyAccessTree propertyAccessTree = ExpressionPropertyAnalyzer.Analyze(propertyAccessor);
            if (propertyAccessTree.DoesEntireTreeSupportINotifyPropertyChanging)
            {
                if (_propertyChangeSubscribers == null)
                {
                    _propertyChangeSubscribers = new List<IPropertyAccessTreeSubscriber<DependsOnMethod<T>>>();
                }

                var subscriber = propertyAccessTree.CreateCallbackSubscription<DependsOnMethod<T>>(OnAnyPropertyInSubscriptionChanges);

                _propertyChangeSubscribers.Add(subscriber);
            }
            else
            {
                if (_accessTreesWithoutNotifyPropertyChanging == null)
                {
                	_accessTreesWithoutNotifyPropertyChanging = new List<PropertyAccessTree>();
                }
                _accessTreesWithoutNotifyPropertyChanging.Add(propertyAccessTree);
            }
            
            return this;
        }

        private static void OnAnyPropertyInSubscriptionChanges(DependsOnMethod<T> me, object objectThatChanged)
        {
            me._callback((T)objectThatChanged);
        }

        internal void CreateSubscriptions(INotifyPropertyChanged subject, ref List<SubscriptionTree> listToAppendTo)
        {
            if (_propertyChangeSubscribers != null)
            {
                for (int i = 0; i < _propertyChangeSubscribers.Count; i++)
                {
                    var propertyChangeSubscriber = _propertyChangeSubscribers[i];
                    propertyChangeSubscriber.SubscribeToChanges(subject, this);
                }
            }

            if(_accessTreesWithoutNotifyPropertyChanging != null)
            {
                if (listToAppendTo == null)
                {
                    listToAppendTo = new List<SubscriptionTree>();
                }

                for (int i = 0; i < _accessTreesWithoutNotifyPropertyChanging.Count; i++)
                {
                    var accessTree = _accessTreesWithoutNotifyPropertyChanging[i];

                    var subscriptionTree = accessTree.CreateSubscriptionTree(subject);

                    listToAppendTo.Add(subscriptionTree);

                    subscriptionTree.PropertyChanged += obj => _callback((T)obj.Parameter);
                }
            }
        }

        #endregion
    }
}
