using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;
using System.Windows;
using System.Collections;
using ContinuousLinq.WeakEvents;
using System.Collections.Specialized;

namespace ContinuousLinq
{
    public interface IPropertyAccessTreeSubscriber<TListener>
    {
        void SubscribeToChanges(INotifyPropertyChanged subject, TListener listener);
        void UnsubscribeFromChanges(INotifyPropertyChanged subject, TListener listener);
    }

    public class PropertyAccessTree
    {
        internal List<PropertyAccessTreeNode> Children { get; set; }

        public PropertyAccessTree()
        {
            this.Children = new List<PropertyAccessTreeNode>();
        }

        public bool IsMonitoringChildProperties
        {
            get { return this.Children.Count != 0 && this.Children[0].Children.Count > 0; }
        }

        public bool DoesEntireTreeSupportINotifyPropertyChanging
        {
            get
            {
                for (int i = 0; i < this.Children.Count; i++)
                {
                    if (!this.Children[i].DoesEntireSubtreeSupportINotifyPropertyChanging)
                        return false;
                }

                return true;
            }
        }
        
        public SubscriptionTree CreateSubscriptionTree(INotifyPropertyChanged parameter)
        {
            List<SubscriptionNode> subscribers = new List<SubscriptionNode>(this.Children.Count);
            for (int i = 0; i < this.Children.Count; i++)
            {
                PropertyAccessTreeNode child = this.Children[i];
                if (child.Children.Count > 0)
                {
                    var subscriptionNode = child.CreateSubscription(parameter);
                    subscribers.Add(subscriptionNode);
                }
            }
            var subscriptionTree = new SubscriptionTree(parameter, subscribers);
            return subscriptionTree;
        }

        public string GetParameterPropertyAccessString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (PropertyAccessTreeNode child in this.Children)
            {
                var childAsParameterNode = child as ParameterNode;

                if (childAsParameterNode == null)
                    continue;

                GetParameterPropertyAccessString(stringBuilder, childAsParameterNode);
            }

            return stringBuilder.ToString();
        }

        private void GetParameterPropertyAccessString(StringBuilder stringBuilder, PropertyAccessTreeNode currentNode)
        {
            foreach (PropertyAccessTreeNode child in currentNode.Children)
            {
                var childAsPropertyAccessNode = child as PropertyAccessNode;

                if (childAsPropertyAccessNode == null)
                    continue;

                if (child.Children.Count > 1)
                {
                    throw new Exception("This property access tree has multiple branches.  Use GetParameterPropertyAccessString only with single branch lambdas like (foo=> foo.Bar) NOT (foo => foo.Bar || foo.Ninja");
                }

                stringBuilder.AppendFormat(
                    stringBuilder.Length == 0 ? "{0}" : ".{0}",
                    childAsPropertyAccessNode.Property.Name);

                GetParameterPropertyAccessString(stringBuilder, childAsPropertyAccessNode);
            }
        }

        public IPropertyAccessTreeSubscriber<TListener> CreateCallbackSubscription<TListener>(Action<TListener, object> onPropertyChanged)
        {
            if (!this.DoesEntireTreeSupportINotifyPropertyChanging)
            {
                throw new Exception();
            }
            
            return new PropertyAccessTreeSubscriberINotifyPropertyChanging<TListener>(this, onPropertyChanged);
        }

        private class PropertyAccessTreeSubscriberINotifyPropertyChanging<TListener> : IPropertyAccessTreeSubscriber<TListener>
        {
            private class SubscriptionNode
            {
                private Action<TListener, object> _subscriberCallback;
                private PropertyAccessNode _propertyAccessNode;

                private Action<TListener, object, object, PropertyChangedEventArgs> _onChangedCallback;
                private Action<TListener, object, object, PropertyChangingEventArgs> _onChangingCallback;

                private List<SubscriptionNode> _children;

                public SubscriptionNode(
                    Action<TListener, object> subscriberCallback,
                    PropertyAccessNode propertyAccessNode)
                {
                    _children = new List<SubscriptionNode>();
                    _subscriberCallback = subscriberCallback;
                    _propertyAccessNode = propertyAccessNode;

                    _onChangedCallback = OnPropertyChanged;
                    _onChangingCallback = OnPropertyChanging;

                    BuildChildren();
                }

                private void BuildChildren()
                {
                    for (int i = 0; i < _propertyAccessNode.Children.Count; i++)
                    {
                        var childSubscriptionNode = new SubscriptionNode(_subscriberCallback, (PropertyAccessNode)_propertyAccessNode.Children[i]);
                        _children.Add(childSubscriptionNode);
                    }
                }

                private void OnPropertyChanging(
                    TListener listener,
                    object subject,
                    object rootSubject,
                    PropertyChangingEventArgs args)
                {
                    UnsubscribeFromChildren(listener, subject, rootSubject);
                }

                private void OnPropertyChanged(
                    TListener listener, 
                    object subject, 
                    object rootSubject, 
                    PropertyChangedEventArgs args)
                {
                    SubscribeToChildren(listener, subject, rootSubject);
                    _subscriberCallback(listener, rootSubject);
                }

                public void Subscribe(
                    INotifyPropertyChanged subject,
                    object rootSubject,
                    TListener listener)
                {
                    WeakPropertyChangedEventManager.Register(
                        subject,
                        rootSubject,
                        _propertyAccessNode.PropertyName,
                        listener,
                        _onChangingCallback,
                        _onChangedCallback);

                    SubscribeToChildren(listener, subject, rootSubject);
                }

                private void SubscribeToChildren(TListener listener, object subject, object rootSubject)
                {
                    if (_children.Count == 0)
                        return;

                    INotifyPropertyChanged newChildSubject = (INotifyPropertyChanged)_propertyAccessNode.GetPropertyValue(subject);

                    if (newChildSubject == null)
                    {
                        return;
                    }

                    for (int i = 0; i < _children.Count; i++)
                    {
                        _children[i].Subscribe(newChildSubject, rootSubject, listener);
                    }
                }

                public void Unsubscribe(
                    INotifyPropertyChanged subject,
                    object rootSubject,
                    TListener listener)
                {
                    WeakPropertyChangedEventManager.Unregister(subject, _propertyAccessNode.PropertyName, listener, rootSubject);

                    UnsubscribeFromChildren(listener, subject, rootSubject);
                }

                private void UnsubscribeFromChildren(TListener listener, object subject, object rootSubject)
                {
                    if (_children.Count == 0)
                        return;

                    INotifyPropertyChanged oldChildSubject = (INotifyPropertyChanged)_propertyAccessNode.GetPropertyValue(subject);

                    if (oldChildSubject == null)
                    {
                        return;
                    }

                    for (int i = 0; i < _children.Count; i++)
                    {
                        _children[i].Unsubscribe(oldChildSubject, rootSubject, listener);
                    }
                }
            }

            private class RootSubscription
            {
                private Action<TListener, object> _subscriberCallback;
                private PropertyAccessTreeNode _propertyAccessTreeNode;
                private List<SubscriptionNode> _children;

                public RootSubscription(
                    Action<TListener, object> subscriberCallback,
                    PropertyAccessTreeNode parameterNode)
                {
                    _propertyAccessTreeNode = parameterNode;
                    _children = new List<SubscriptionNode>();
                    _subscriberCallback = subscriberCallback;

                    BuildChildren(parameterNode);
                }

                private void BuildChildren(PropertyAccessTreeNode root)
                {
                    for (int i = 0; i < root.Children.Count; i++)
                    {
                        var childSubscriptionNode = new SubscriptionNode(_subscriberCallback, (PropertyAccessNode)root.Children[i]);
                        _children.Add(childSubscriptionNode);
                    }
                }

                public void Subscribe(
                  INotifyPropertyChanged subject,
                  TListener listener)
                {
                    INotifyPropertyChanged subjectToSubscribe = subject;
                    ConstantNode constantNode = _propertyAccessTreeNode as ConstantNode;
                    if (constantNode != null)
                    {
                        subjectToSubscribe = constantNode.Value;
                    }

                    for (int i = 0; i < _children.Count; i++)
                    {
                        var child = _children[i];

                        child.Subscribe(subjectToSubscribe, subject, listener);
                    }
                }

                public void Unsubscribe(
                    INotifyPropertyChanged subject,
                    TListener listener)
                {
                    INotifyPropertyChanged subjectToUnsubscribe = subject;
                    ConstantNode constantNode = _propertyAccessTreeNode as ConstantNode;
                    if (constantNode != null)
                    {
                        subjectToUnsubscribe = constantNode.Value;
                    }

                    for (int i = 0; i < _children.Count; i++)
                    {
                        _children[i].Unsubscribe(subjectToUnsubscribe, subject, listener);
                    }
                }
            }

            private List<RootSubscription> _subscriptions;

            private PropertyAccessTree _propertyAccessTree;

            public PropertyAccessTreeSubscriberINotifyPropertyChanging(
                PropertyAccessTree propertyAccessTree,
                Action<TListener, object> subscriberCallback)
            {
                _subscriptions = new List<RootSubscription>();

                _propertyAccessTree = propertyAccessTree;

                for (int i = 0; i < propertyAccessTree.Children.Count; i++)
                {
                    var rootSubscription = new RootSubscription(subscriberCallback, propertyAccessTree.Children[i]);
                    _subscriptions.Add(rootSubscription);
                }
            }

            public void SubscribeToChanges(
                INotifyPropertyChanged subject,
                TListener listener)
            {
                for (int i = 0; i < _subscriptions.Count; i++)
                {
                    _subscriptions[i].Subscribe(subject, listener);
                }
            }

            public void UnsubscribeFromChanges(
                INotifyPropertyChanged subject,
                TListener listener)
            {
                for (int i = 0; i < _subscriptions.Count; i++)
                {
                    _subscriptions[i].Unsubscribe(subject, listener);
                }
            }
        }
    }
}