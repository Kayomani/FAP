using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookLibrary.Applications.Controllers;
using System.ComponentModel.Composition;

namespace BookLibrary.Applications.Test.Controllers
{
    [Export(typeof(IPresentationController)), Export]
    public class PresentationControllerMock : IPresentationController
    {
        public bool InitializeCulturesCalled { get; set; }
        

        public void InitializeCultures()
        {
            InitializeCulturesCalled = true;
        }
    }
}
