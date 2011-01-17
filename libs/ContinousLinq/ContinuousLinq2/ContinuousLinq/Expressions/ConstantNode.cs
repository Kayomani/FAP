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
    internal class ConstantNode : PropertyAccessTreeNode
    {
        public INotifyPropertyChanged Value { get; set; }

        public override Type Type
        {
            get { return this.Value.GetType(); }
        }

        public ConstantNode(INotifyPropertyChanged value)
        {
            this.Value = value;
        }

        public override bool IsRedundantVersion(PropertyAccessTreeNode other)
        {
            ConstantNode otherAsConstantNode = other as ConstantNode;
            if (otherAsConstantNode == null)
                return false;

            return other != this && otherAsConstantNode.Value == this.Value;
        }

        public override SubscriptionNode CreateSubscription(INotifyPropertyChanged parameter)
        {
            var subscriptionNode = new SubscriptionNode()
            { 
                AccessNode = this 
            };
            SubscribeToChildren(subscriptionNode, parameter);

            subscriptionNode.Subject = this.Value;

            return subscriptionNode;
        }
    }
}
