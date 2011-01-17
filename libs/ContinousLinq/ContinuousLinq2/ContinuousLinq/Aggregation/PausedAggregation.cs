using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Collections.Specialized;

namespace ContinuousLinq.Aggregates
{
    /// <summary>
    /// Usage pattern: When you wish to guard a block of code such that no background re-aggregation of data
    /// takes place during that block of code, you can enclose your block in a using as follows:
    /// <code>
    /// using (PausedAggregation p = new PausedAggregation) { ... guarded code ... }
    /// </code>
    /// </summary>
    public class PausedAggregation : IDisposable
    {
        private bool _wasDisposed;

        public PausedAggregation()
        {
            ContinuousValue.GlobalPause();
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (!_wasDisposed)
            {
                ContinuousValue.GlobalResume();
                _wasDisposed = true;
            }
        }
        #endregion
    }
}
