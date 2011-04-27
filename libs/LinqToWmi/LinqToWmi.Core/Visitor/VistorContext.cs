using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Linq;

namespace LinqToWmi.Core.WMI
{
    /// <summary>
    /// Represents an context voor the visitor to work with. The visitors are registered
    /// using the RegisterVisitor method. When child visitors call the Visit(Expression ex) method
    /// on the context it automatically fetches the correct visitor (if any) 
    /// </summary>
    class VistorContext
    {
        public List<IVisitor> _visitors = new List<IVisitor>();

        /// <summary>
        /// Register an expression handeling visitor for an expression
        /// </summary>
        public void RegisterVisitor<T>() where T : IVisitor, new()
        {
            T visitor = new T();
            visitor.Context = this;
            _visitors.Add(visitor);
        }

        /// <summary>
        /// Perform an visit for an expression
        /// </summary>
        internal string Visit(Expression expression)
        {
            IVisitor visitor = null;

            //try
            //{
                visitor = (from v in _visitors
                           where v.ExpressionType == expression.GetType()
                           select v).First();
            //}
            //catch (EmptySequenceException)
            //{
            //    throw new NotImplementedException(String.Format("The vistor for {0} is either not implemented or registered",
            //        expression.GetType().Name));
            //}

            return visitor.Visit(expression);
        }
    }
}
