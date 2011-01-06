using System;
using System.Linq.Expressions;

namespace LinqToWmi.Core.WMI
{
    /// <summary>
    /// Basic Visitor interface to work with
    /// </summary>
    interface IVisitor
    {        
        Type ExpressionType { get;  }
        string Visit(Expression ex);
        VistorContext Context { get; set; }
    }
}
