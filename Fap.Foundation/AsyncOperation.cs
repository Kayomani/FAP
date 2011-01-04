using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;

namespace Fap.Foundation
{
    public class AsyncOperation
    {
        public DelegateCommand Command { set; get; }
        public Object Object { set; get; }
        public DelegateCommand CompletedCommand { set; get; }
    }
}
