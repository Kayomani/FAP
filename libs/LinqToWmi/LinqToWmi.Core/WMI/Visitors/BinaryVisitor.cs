using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace LinqToWmi.Core.WMI
{
    /// <summary>
    /// Visit an binary expression. Binary expressions are used for equations,
    /// since the left or the right part can be of any type we just recurse.
    /// </summary>
    internal class BinaryVisitor : AbstractVisitor<BinaryExpression>
    {
        public override string Visit(BinaryExpression expression)
        {
            string where = null;

            where += Context.Visit(expression.Left);
            where += WmiQueryUtility.ConvertBinaryTypeToSql(expression.NodeType);

            if (expression.Right.Type == typeof(String))
            {
                where += String.Format("'{0}'", Context.Visit(expression.Right));
            }
            else
            {
                where += Context.Visit(expression.Right);
            }

            return where;
        }
    }
}