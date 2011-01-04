using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Writer.Applications.Controllers;
using System.ComponentModel.Composition;

namespace Writer.Applications.Test.Controllers
{
    [Export(typeof(IPresentationController))]
    public class PresentationControllerMock : IPresentationController
    {
        public void InitializeCultures()
        {
        }
    }
}
