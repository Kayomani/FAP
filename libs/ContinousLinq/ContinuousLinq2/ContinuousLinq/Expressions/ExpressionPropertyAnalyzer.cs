using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;

namespace ContinuousLinq
{
    public static class ExpressionPropertyAnalyzer
    {
        #region Methods

        public static PropertyAccessTree Analyze<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            return Analyze(expression, DoesTypeImplementINotifyPropertyChanged);
        }

        public static PropertyAccessTree Analyze<T, TResult>(Expression<Func<T, TResult>> expression, Predicate<Type> typeFilter)
        {
            if (!typeFilter(typeof(T)))
            {
                return null;
            }

            PropertyAccessTree tree = AnalyzeLambda(expression, typeFilter);
            return tree;
        }

        public static PropertyAccessTree Analyze<T0, T1, TResult>(Expression<Func<T0, T1, TResult>> expression)
        {
            PropertyAccessTree tree = AnalyzeLambda(expression, DoesTypeImplementINotifyPropertyChanged);
            return tree;
        }

        private static PropertyAccessTree AnalyzeLambda(LambdaExpression expression, Predicate<Type> typeFilter)
        {
            PropertyAccessTree tree = new PropertyAccessTree();
            //This is done to ensure that the tree has all the parameters and in the same order.
            for (int i = 0; i < expression.Parameters.Count; i++)
            {
                ParameterExpression parameterExpression = expression.Parameters[0];
                tree.Children.Add(new ParameterNode(parameterExpression.Type, parameterExpression.Name));
            }
            BuildUnoptimizedTree(tree, expression.Body, typeFilter);

            RemoveRedundantNodesFromTree(tree.Children);
            ApplyTypeFilter(tree.Children, typeFilter);
        
            return tree;
        }

        private static void ApplyTypeFilter(List<PropertyAccessTreeNode> children, Predicate<Type> typeFilter)
        {
            for (int i = children.Count - 1; i >= 0; i--)
            {
                var propertyAccessNode = children[i] as PropertyAccessNode;
                if (propertyAccessNode != null)
                {
                    if (propertyAccessNode.Children.Count > 0 && !typeFilter(propertyAccessNode.Property.PropertyType))
                    {
                        propertyAccessNode.Children.Clear();
                    }
                }
                ApplyTypeFilter(children[i].Children, typeFilter);
            }
        }

        private static void RemoveRedundantNodesFromTree(IList<PropertyAccessTreeNode> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = nodes.Count - 1; j > i; j--)
                {
                    if (nodes[i].IsRedundantVersion(nodes[j]))
                    {
                    	nodes[i].Children.AddRange(nodes[j].Children);
                        nodes.RemoveAt(j);
                    }
                }
                RemoveRedundantNodesFromTree(nodes[i].Children);
            }
        }

        private static void BuildUnoptimizedTree(PropertyAccessTree tree, Expression expression, Predicate<Type> typeFilter)
        {
            var currentNodeBranch = new Stack<PropertyAccessTreeNode>();
            BuildBranches(expression, tree, currentNodeBranch, typeFilter);
        }

        private static bool DoesTypeImplementINotifyPropertyChanged(Type type)
        {
            return typeof(INotifyPropertyChanged).IsAssignableFrom(type);
        }

        private static void BuildBranches(Expression expression, PropertyAccessTree tree, Stack<PropertyAccessTreeNode> currentNodeBranch, Predicate<Type> typeFilter)
        {
            BinaryExpression binaryExpression = expression as BinaryExpression;

            if (binaryExpression != null)
            {
                BuildBranches(binaryExpression.Left, tree, currentNodeBranch, typeFilter);
                BuildBranches(binaryExpression.Right, tree, currentNodeBranch, typeFilter);
                return;
            }

            UnaryExpression unaryExpression = expression as UnaryExpression;

            if (unaryExpression != null)
            {
                BuildBranches(unaryExpression.Operand, tree, currentNodeBranch, typeFilter);
                return;
            }

            MethodCallExpression methodCallExpression = expression as MethodCallExpression;

            if (methodCallExpression != null)
            {
                foreach (Expression argument in methodCallExpression.Arguments)
                {
                    BuildBranches(argument, tree, currentNodeBranch, typeFilter);
                }
                return;
            }

            ConditionalExpression conditionalExpression = expression as ConditionalExpression;

            if (conditionalExpression != null)
            {
                BuildBranches(conditionalExpression.Test, tree, currentNodeBranch, typeFilter);
                BuildBranches(conditionalExpression.IfTrue, tree, currentNodeBranch, typeFilter);
                BuildBranches(conditionalExpression.IfFalse, tree, currentNodeBranch, typeFilter);
                return;
            }

            InvocationExpression invocationExpression = expression as InvocationExpression;

            if (invocationExpression != null)
            {
                foreach (Expression argument in invocationExpression.Arguments)
                {
                    BuildBranches(argument, tree, currentNodeBranch, typeFilter);
                }
                BuildBranches(invocationExpression.Expression, tree, currentNodeBranch, typeFilter);
                return;
            }

            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    MemberExpression memberExpression = (MemberExpression)expression;

                    PropertyInfo property = memberExpression.Member as PropertyInfo;
                    FieldInfo fieldInfo = memberExpression.Member as FieldInfo;
                    if (property != null)
                    {
                        PropertyAccessNode node = new PropertyAccessNode(property);
                        currentNodeBranch.Push(node);

                        BuildBranches(memberExpression.Expression, tree, currentNodeBranch, typeFilter);
                    }
                    else if (fieldInfo != null)
                    {
                        if (typeFilter(fieldInfo.FieldType))
                        {
                            ConstantExpression constantExpression = (ConstantExpression)memberExpression.Expression;
                            if (constantExpression.Value != null)
                            {
                                object value = fieldInfo.GetValue(constantExpression.Value);
                                ConstantNode constantNode = new ConstantNode((INotifyPropertyChanged)value);
                                currentNodeBranch.Push(constantNode);
                                AddBranch(tree, currentNodeBranch);
                            }
                        }
                        else
                        {
                            currentNodeBranch.Clear();
                        }
                    }
                    else
                    {
                        BuildBranches(memberExpression.Expression, tree, currentNodeBranch, typeFilter);
                    }

                    break;

                case ExpressionType.Parameter:
                    ParameterExpression parameterExpression = (ParameterExpression)expression;
                    ParameterNode parameterNode = new ParameterNode(expression.Type, parameterExpression.Name);
                    currentNodeBranch.Push(parameterNode);
                    AddBranch(tree, currentNodeBranch);
                    break;

                case ExpressionType.Constant:
                    {
                        ConstantExpression constantExpression = (ConstantExpression)expression;
                        if (typeFilter(constantExpression.Type) &&
                            constantExpression.Value != null)
                        {
                            ConstantNode constantNode = new ConstantNode((INotifyPropertyChanged)constantExpression.Value);
                            currentNodeBranch.Push(constantNode);
                            AddBranch(tree, currentNodeBranch);
                        }
                        else
                        {
                            currentNodeBranch.Clear();
                        }
                    }
                    break;
                case ExpressionType.New:
                    {
                        NewExpression newExpression = (NewExpression)expression;
                        foreach (Expression argument in newExpression.Arguments)
                        {
                            BuildBranches(argument, tree, currentNodeBranch, typeFilter);
                        }
                    }
                    break;
                case ExpressionType.MemberInit:
                    {
                        MemberInitExpression memberInitExpression = (MemberInitExpression)expression;
                        BuildBranches(memberInitExpression.NewExpression, tree, currentNodeBranch, typeFilter);
                        foreach (var memberBinding in memberInitExpression.Bindings)
                        {
                            MemberAssignment assignment = memberBinding as MemberAssignment;
                            if (assignment != null)
                            {
                                BuildBranches(assignment.Expression, tree, currentNodeBranch, typeFilter);
                            }
                        }
                    }
                    break;
                default:
                    throw new InvalidProgramException(string.Format("CLINQ does not support expressions of type: {0}", expression.NodeType));
            }
        }

        private static void AddBranch(PropertyAccessTree tree, Stack<PropertyAccessTreeNode> currentNodeBranch)
        {
            if (currentNodeBranch.Count == 0)
                return;

            PropertyAccessTreeNode currentNode = currentNodeBranch.Pop();
            tree.Children.Add(currentNode);

            while (currentNodeBranch.Count != 0)
            {
                PropertyAccessTreeNode nextNode = currentNodeBranch.Pop();
                currentNode.Children.Add(nextNode);
                currentNode = nextNode;
            }
        }

        #endregion
    }
}


