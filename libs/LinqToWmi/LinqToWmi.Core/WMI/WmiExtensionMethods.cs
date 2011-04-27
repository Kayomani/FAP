using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

namespace LinqToWmi.Core.WMI
{
   /// <summary>
    /// Create some extension methods so that the compiler can do its sexy type deferrence. IE. 
    /// Lookup compatible methods for building up the expression tree. 
    /// </summary>  
    public static class WmiExtensions
    {
        //Note the "Expression" syntax, it triggers the compiler to convert this to an expression tree.
        //We want that tree so we store it in the Query
        public static WmiQuery<T> Where<T>(this WmiQuery<T> context, Expression<Func<T, bool>> expression)
        {   
            context.WhereExpression = expression.Body;

            return context;
        }

        

    }
}
