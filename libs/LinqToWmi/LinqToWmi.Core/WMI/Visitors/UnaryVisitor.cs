using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using Ex = System.Linq.Expressions;

namespace LinqToWmi.Core.WMI
{
    /// <summary>
    /// Unary expressions are the kind of expressions that are written like !o.QuotasDisabled
    /// </summary>
    internal class UnaryVisitor : AbstractVisitor<UnaryExpression>
    {
        Dictionary<ExpressionType, Func<UnaryExpression, string>> operands = new Dictionary<ExpressionType, Func<UnaryExpression, string>>();

        public UnaryVisitor() {
            operands.Add(Ex.ExpressionType.Not, HandleNotNodeType);
        }

        public override string Visit(UnaryExpression expression)
        {
            if(operands.ContainsKey(expression.NodeType)) {
                return operands[expression.NodeType](expression);
            }
            throw new NotImplementedException("Unary operand type not implemented " + expression.NodeType);
        }

        /// <summary>
        /// This is cheating, the most ideal way of converting expression trees is to convert
        /// the LINQ syntax first to a custom tree and parse that, and covert the standalone
        /// boolean member expressions to binary expressions. Because of a LINQ defect
        /// the 
        /// </summary>
        private string HandleNotNodeType(UnaryExpression expression) {

            MemberExpression member = expression.Operand as MemberExpression;
            
            if(member != null) {
                PropertyInfo property = member.Member as PropertyInfo;
                if(property != null && property.PropertyType == typeof(Boolean)) {
                    return WmiQueryUtility.ConvertBinaryBooleanToSQL(member.Member, false);                    
                }
            }
            return Context.Visit(expression.Operand);
        }
    }
}