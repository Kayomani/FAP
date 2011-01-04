using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;

namespace EmailClient.Applications.Test.Mocks
{
    public class ViewMock : IView
    {
        public object DataContext { get; set; }
    }
}
