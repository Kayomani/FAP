using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqToWmi.Core.WMI
{
    /// <summary>
    /// Converts an WmiQuery member to an WQL field
    /// </summary>
    internal class MemberVisitor : AbstractVisitor<MemberExpression>
    {
        public override string Visit(MemberExpression expression)
        {
            return ConvertCommonOperations(expression);
        }

        /// <summary>
        /// Convert common operation for an Member
        /// </summary>
        public string ConvertCommonOperations(MemberExpression expression)
        {
            PropertyInfo property = expression.Member as PropertyInfo;

            if (property != null && property.PropertyType == typeof(bool))
            {
                return WmiQueryUtility.ConvertBinaryBooleanToSQL(expression.Member, true);                
            }

            return WmiQueryUtility.ConvertMemberToSql(expression.Member);
        }
    }
}