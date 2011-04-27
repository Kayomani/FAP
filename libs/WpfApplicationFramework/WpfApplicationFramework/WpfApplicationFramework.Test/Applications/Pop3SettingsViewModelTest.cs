using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Waf.Foundation;
using System.Waf.Applications;
using System.ComponentModel;
using System.Waf.UnitTesting;

namespace Test.Waf.Applications
{
    [TestClass]
    public class Pop3SettingsViewModelTest
    {
        public TestContext TestContext { get; set; }


        [TestMethod]
        public void ViewModelTest()
        {
            Pop3SettingsViewMock viewMock = new Pop3SettingsViewMock();
            Pop3Settings pop3Settings = new Pop3Settings();
            Pop3SettingsViewModel viewModel = new Pop3SettingsViewModel(viewMock, pop3Settings);

            Assert.AreEqual(viewMock, viewModel.View);

            Assert.AreEqual(viewModel, viewMock.DataContext);

            Assert.IsFalse(viewModel.Pop3SettingsServerPathChanged);
            pop3Settings.ServerPath = "pop.mail.com";
            Assert.IsTrue(viewModel.Pop3SettingsServerPathChanged);
        }


        [TestMethod]
        public void ConstructorParameterTest()
        {
            AssertHelper.ExpectedException<ArgumentNullException>(() => new Pop3SettingsViewModel(null, null));
        }


        private class Pop3SettingsViewModel : ViewModel<IPop3SettingsView>
        {
            private readonly Pop3Settings pop3Settings;


            public Pop3SettingsViewModel(IPop3SettingsView view, Pop3Settings pop3Settings) : base(view)
            {
                this.pop3Settings = pop3Settings;
                AddWeakEventListener(pop3Settings, Pop3SettingsPropertyChanged);
            }


            public bool Pop3SettingsServerPathChanged { get; set; }


            private void Pop3SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "ServerPath")
                {
                    Pop3SettingsServerPathChanged = true;
                }
            }
        }

        private interface IPop3SettingsView : IView { }

        private class Pop3SettingsViewMock : IPop3SettingsView
        {
            public object DataContext { get; set; }
        }

        private class Pop3Settings : Model
        {
            private string serverPath;


            public string ServerPath
            {
                get { return serverPath; }
                set
                {
                    if (serverPath != value)
                    {
                        serverPath = value;
                        RaisePropertyChanged("ServerPath");
                    }
                }
            }
        }
    }
}
