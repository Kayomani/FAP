using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;
using System.Windows;
using System.Collections;
using ContinuousLinq.Expressions;

namespace ContinuousLinq
{
    internal class PropertyAccessNode : PropertyAccessTreeNode
    {
        public PropertyInfo Property { get; private set; }

        public string PropertyName { get { return this.Property.Name; } }

        public override Type Type
        {
            get { return this.Property.PropertyType; }
        }

        private DynamicProperty _dynamicProperty;

        public PropertyAccessNode(PropertyInfo property)
        {
            this.Property = property;
        }

        public override bool IsRedundantVersion(PropertyAccessTreeNode other)
        {
            var otherAsPropertyNode = other as PropertyAccessNode;
            if (otherAsPropertyNode == null)
                return false;

            return other != this && otherAsPropertyNode.Property == this.Property;
        }

        public override SubscriptionNode CreateSubscription(INotifyPropertyChanged parameter)
        {
            var subscriptionNode = new SubscriptionNode()
            {
                AccessNode = this,
            };

            SubscribeToChildren(subscriptionNode, parameter);

            return subscriptionNode;
        }

        public object GetPropertyValue(object obj)
        {
            if (_dynamicProperty == null)
                _dynamicProperty = DynamicProperty.Create(this.Property);

            return _dynamicProperty.GetValue(obj);
        }

        public override string ToString()
        {
            return string.Format("Node: {0}", this.Property.ToString());
        }
    }
}
