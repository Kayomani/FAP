using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

namespace LinqToWmi.Core.WMI
{
    /// <summary>
    /// Abstract visitor, does some neat casting for you, and some other tricks that
    /// make life easier
    /// </summary>
    abstract class AbstractVisitor<T> : IVisitor where T: Expression
    {
        public abstract string Visit(T element);
        private VistorContext _context = null;

        public Type ExpressionType
        {
            get
            {
                return typeof(T);
            }
        }  

        public string Visit(Expression ex)
        {
           return Visit((T)ex);
        }      

        public VistorContext Context
        {
            get { 
                return _context;
            }
            set {
                _context = value;
            }
        }
    }
}
