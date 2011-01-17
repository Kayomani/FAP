using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;
using System.Windows;
using System.Collections;

namespace ContinuousLinq
{
    internal abstract class PropertyAccessTreeNode
    {

        public abstract bool IsRedundantVersion(PropertyAccessTreeNode other);
        public abstract Type Type { get; }
        
        public List<PropertyAccessTreeNode> Children { get; set; }

        public bool DoesEntireSubtreeSupportINotifyPropertyChanging
        {
            get
            {
                if (this.Children.Count == 0)
                    return true;

                if (!typeof(INotifyPropertyChanging).IsAssignableFrom(this.Type))
                    return false;

                for (int i = 0; i < this.Children.Count; i++)
                {
                    if (!this.Children[i].DoesEntireSubtreeSupportINotifyPropertyChanging)
                        return false;
                }

                return true;
            }
        }

        public PropertyAccessTreeNode()
        {
            this.Children = new List<PropertyAccessTreeNode>();
        }

        public abstract SubscriptionNode CreateSubscription(INotifyPropertyChanged parent);

        protected void SubscribeToChildren(SubscriptionNode subscriptionNode, INotifyPropertyChanged parameter)
        {
            for (int i = 0; i < this.Children.Count; i++)
            {
                PropertyAccessTreeNode child = this.Children[i];
                if (child.Children.Count == 0)
                    continue;

                SubscriptionNode childSubscriptionNode = child.CreateSubscription(parameter);

                if (subscriptionNode.Children == null)
                    subscriptionNode.Children = new List<SubscriptionNode>();

                subscriptionNode.Children.Add(childSubscriptionNode);
            }
        }
    }
}
