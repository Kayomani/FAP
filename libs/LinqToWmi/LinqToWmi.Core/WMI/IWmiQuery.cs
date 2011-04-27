using System;
using System.Linq.Expressions;

namespace LinqToWmi.Core.WMI
{
    /// <summary>
    /// Generic interface for WmiQuery provides access to Expression and Type for our generic concrete types.
    /// </summary>
    public interface IWmiQuery
    {
        Expression WhereExpression { get; set; }

        Type Type { get; }	
    }
}
