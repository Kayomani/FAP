using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;

namespace EmailClient.Applications.Test.Mocks
{
    public class ViewMock<TViewModel> : ViewMock where TViewModel : ViewModel
    {
        public TViewModel ViewModel { get { return this.GetViewModel<TViewModel>(); } }
    }
}
