using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;

namespace EmailClient.Applications.Views
{
    public interface IEmailAccountsView : IView
    {
        bool? ShowDialog(object owner);
        
        void Close();
    }
}
