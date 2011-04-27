using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Linq.Expressions;
using System.Diagnostics;

//TODO: Look at the WQL Reference at http://msdn2.microsoft.com/en-us/library/aa394606.aspx and add important missing features

namespace LinqToWmi.Core.WMI
{
    /// <summary>
    /// Represents an single WMI query, we derive from IWMIQuery so that we 
    /// can easily get the most important properties without having the exact generic type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WmiQuery<T> : IEnumerable<T>, IWmiQuery
    {
        private Expression _expressionWhere = null;
        private WmiContext _context = null;
        private WmiQueryBuilder _builder = null;

        public WmiQuery(WmiContext context)
        {
            _context = context;
            _builder = new WmiQueryBuilder(context);
        }

        /// <summary>
        /// The LINQ where clause
        /// </summary>
        public Expression WhereExpression
        {
            get
            {
                return _expressionWhere;
            }
            set
            {
                _expressionWhere = value;
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return (IEnumerator<T>)_context.ExecuteWmiQuery(this);
        }

        /// <summary>
        /// Executes the Wmi query
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return _context.ExecuteWmiQuery(this);
        }

        /// <summary>
        /// The WMI object type being queried
        /// </summary>
        public Type Type
        {
            get
            {
                return typeof(T);
            }
        }
    }
}
