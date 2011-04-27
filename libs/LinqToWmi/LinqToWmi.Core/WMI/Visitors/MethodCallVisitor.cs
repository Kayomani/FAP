using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace LinqToWmi.Core.WMI
{
    /// <summary>
    /// Visit an binary expression. Binary expressions are used for equations,
    /// since the left or the right part can be of any expression we recurse.
    /// </summary>
    internal class MethodCallVisitor : AbstractVisitor<MethodCallExpression>
    {
        public override string Visit(MethodCallExpression expression)
        {
            return TranslateCommonOperations(expression);
        }

        /// <summary>
        /// Certain method calls can be translated to its WQL counterpart
        /// for instance String.contains can be converted to Like '%%' or String.Equals => '=' 
        /// </summary>
        public string TranslateCommonOperations(MethodCallExpression exp)
        {
            if (exp.Method.DeclaringType == typeof(String))
            {
                switch (exp.Method.Name)
                {
                    case "op_Equality":
                        return String.Format("{0} = '{1}'", Context.Visit(exp.Arguments[0]), Context.Visit(exp.Arguments[1]));
                    case "Contains":
                        return String.Format("{0} LIKE '%{1}%'", Context.Visit(exp.Object), Context.Visit(exp.Arguments[0]));
                    case "op_Inequality":
                        return String.Format("{0} <> '{1}'", Context.Visit(exp.Arguments[0]), Context.Visit(exp.Arguments[1]));
                }
            }

            return null;
        }
    }
}