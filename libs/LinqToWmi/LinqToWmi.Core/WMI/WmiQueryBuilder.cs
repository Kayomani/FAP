using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace LinqToWmi.Core.WMI
{
    /// <summary>
    /// Simple class for creating WMI Queries, we're not doing the full monty cause that would be an idiotic
    /// and moreover and daunting task that would actually require some work (which im trying to
    /// avoid at every cost:)
    /// </summary>
    public class WmiQueryBuilder
    {
        VistorContext whereVisitor = new VistorContext();

        public WmiQueryBuilder(WmiContext context)
        {
            RegisterWhereVisitors();
        }

        /// <summary>
        /// Register the where visitors 
        /// </summary>
        private void RegisterWhereVisitors()
        {
            whereVisitor.RegisterVisitor<UnaryVisitor>();
            whereVisitor.RegisterVisitor<BinaryVisitor>();
            whereVisitor.RegisterVisitor<MemberVisitor>();
            whereVisitor.RegisterVisitor<ConstantVisitor>();
            whereVisitor.RegisterVisitor<MethodCallVisitor>();
        }

        /// <summary>
        /// Builds an query that can be used with SQL
        /// </summary>
        public string BuildQuery(IWmiQuery queryExpression)
        {
            string query = String.Format("SELECT * FROM {0}", WmiMapper.ConvertTableName(queryExpression.Type));

            if (queryExpression.WhereExpression != null)
            {
                query = String.Format("{0} WHERE {1}", query, BuildWhereClause(queryExpression.WhereExpression));
            }          

            return query;
        }
        
        /// <summary>
        /// Build the WHERE part of the query
        /// </summary>
        public string BuildWhereClause(Expression ex)
        {
            return whereVisitor.Visit(ex);
        }
    }
}