using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;

namespace FAP.Application.Views
{
    public interface IWebPanel: IView
    {
        string Location { set; get; }
    }
}
