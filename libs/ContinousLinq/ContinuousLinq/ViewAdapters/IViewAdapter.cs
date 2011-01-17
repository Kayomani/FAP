using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ContinuousLinq
{
    public interface IViewAdapter
    {
        IViewAdapter PreviousAdapter { get; }
        void ReEvaluate();
    }
}
