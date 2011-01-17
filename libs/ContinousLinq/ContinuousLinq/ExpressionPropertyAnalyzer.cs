using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;

namespace ContinuousLinq
{    
    public static class ExpressionPropertyAnalyzer
    {
        public static Dictionary<Type, HashSet<string>> GetReferencedPropertyNames<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            Dictionary<Type, HashSet<string>> properties = new Dictionary<Type, HashSet<string>>();

            GetReferencedPropertyNames(expression.Body, properties);

            return properties;
        }

        private static void GetReferencedPropertyNames(Expression expression, Dictionary<Type, HashSet<string>> properties)
        {
            BinaryExpression binaryExpression = expression as BinaryExpression;

            if (binaryExpression != null)
            {
                GetReferencedPropertyNames(binaryExpression.Left, properties);
                GetReferencedPropertyNames(binaryExpression.Right, properties);
                return;
            }

            UnaryExpression unaryExpression = expression as UnaryExpression;

            if (unaryExpression != null)
            {
                GetReferencedPropertyNames(unaryExpression.Operand, properties);
                return;
            }

            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    MemberExpression memberExpression = (MemberExpression)expression;

                    PropertyInfo property = memberExpression.Member as PropertyInfo;
                    
                    if (property != null)
                    {
                        HashSet<string> propertyNames;
                        if (!properties.TryGetValue(property.DeclaringType, out propertyNames))
                        {
                            propertyNames = new HashSet<string>();
                            properties[property.DeclaringType] = propertyNames;
                        }

                        propertyNames.Add(property.Name);
                    }
                    break;

                case ExpressionType.Parameter:
                    break;

                default:
                    break;
            }
        }
    }
}


