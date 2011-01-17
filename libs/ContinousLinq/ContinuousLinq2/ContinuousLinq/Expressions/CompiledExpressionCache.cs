using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace ContinuousLinq.Expressions
{
    public static class CompiledExpressionCache
    {
        internal static Dictionary<CompiledExpressionCacheEntry, Delegate> _cache = new Dictionary<CompiledExpressionCacheEntry, Delegate>();

        public static T CachedCompile<T>(this Expression<T> expression)
            where T : class
        {
            var closureTransformer = new ClosedToOpenExpressionTransformer(expression);

            CompiledExpressionCacheEntry entry = new CompiledExpressionCacheEntry(closureTransformer.OpenVersion);

            Delegate compiledExpression;
            if (!_cache.TryGetValue(entry, out compiledExpression))
            {
                compiledExpression = closureTransformer.OpenVersion.Compile() as Delegate;
                _cache[entry] = compiledExpression;
            }

            if (!closureTransformer.WasAlreadyOpen)
            {
                return ClosedToOpenExpressionFactory.CreateMetaClosure<T>(compiledExpression, closureTransformer.Constants);
            }
            else
            {
                return compiledExpression as T; 
            }
        }
    }

    internal class CompiledExpressionCacheEntry
    {
        static ExpressionEqualityComparer _expressionEqualityComparer = new ExpressionEqualityComparer();

        public LambdaExpression Expression { get; set; }

        public int HashCode { get; set; }

        public CompiledExpressionCacheEntry(LambdaExpression expression)
        {
            this.Expression = expression;
            this.HashCode = _expressionEqualityComparer.GetHashCode(expression);
        }

        public override int GetHashCode()
        {
            return this.HashCode;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;
            
            CompiledExpressionCacheEntry objAsEntry = obj as CompiledExpressionCacheEntry;

            return objAsEntry != null && _expressionEqualityComparer.Equals(this.Expression, objAsEntry.Expression);
        }
    }
}
