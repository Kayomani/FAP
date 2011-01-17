using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkLibrary.Implementations;
using ContinuousLinq;

namespace FilteringApplication1
{
    public class BinSplit
    {
        public ContinuousCollection<Transaction> LeftBin { get; set; }
        public ContinuousCollection<Transaction> RightBin { get; set; }
    }
}
