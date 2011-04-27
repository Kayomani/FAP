using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Waf.Applications;

namespace Test.Waf.Applications
{
    [TestClass]
    public class ViewModelTest
    {
        [TestMethod]
        public void GetViewTest() 
        {
            IView view = new ViewMock();
            DummyViewModel viewModel = new DummyViewModel(view);

            Assert.AreEqual(view, viewModel.View);
        }



        private class DummyViewModel : ViewModel
        {
            public DummyViewModel(IView view) : base(view)
            {
            }
        }


        private class ViewMock : IView
        {
            public object DataContext { get; set;}
        }
    }
}
