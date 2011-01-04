using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;

namespace ShortcutKeySample.Applications
{
    internal interface IShellView : IView
    {
        void Show();
    }
}
