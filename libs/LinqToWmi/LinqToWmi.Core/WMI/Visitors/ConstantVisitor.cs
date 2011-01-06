using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace LinqToWmi.Core.WMI
{
    /// <summary>
    /// Visits constant expression, constant expressions are just resolved
    /// variables, like ints or strings which are put directly in the query
    /// </summary>
    internal class ConstantVisitor : AbstractVisitor<ConstantExpression>
    {
        public override string Visit(ConstantExpression expression)
        {
            if (expression.Value == null)
            {
                return "NULL";
            }
            else
            {
                return expression.Value.ToString();
            }
        }
    }
}