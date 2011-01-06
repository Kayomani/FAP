using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using LinqToWmi.Core.WMI;

namespace LinqToWmi.Core.WMI
{
    /// <summary>
    /// Simple WmiQuery utility for handing various Query tasks
    /// </summary>
    internal class WmiQueryUtility
    {
        public static string ConvertBinaryTypeToSql(ExpressionType type)
        {

            switch (type)
            {
                case ExpressionType.GreaterThan:
                    return " > ";
                case ExpressionType.GreaterThanOrEqual:
                    return " >= ";
                case ExpressionType.LessThan:
                    return " < ";
                case ExpressionType.LessThanOrEqual:
                    return " <= ";
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return " AND ";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return " OR ";
                case ExpressionType.Equal:
                    return " = ";
                case ExpressionType.NotEqual:
                    return " <> ";
            }

            throw new NotImplementedException("The expression type: " + type);
        }

        /// <summary>
        /// Converts an member to an SQL syntax
        /// </summary>
        public static string ConvertMemberToSql(MemberInfo memberInfo)
        {
            return WmiMapper.ConvertMemberName(memberInfo);
        }

        /// <summary>
        /// Converts an binary where boolean expression to an SQL syntax
        /// </summary>
        public static string ConvertBinaryBooleanToSQL(MemberInfo memberInfo, bool value)
        {
            return string.Format("{0} = {1}", WmiQueryUtility.ConvertMemberToSql(memberInfo), value);
        }
    }
}
