using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmailClient.Applications.ViewModels;
using EmailClient.Applications.Test.Mocks;
using EmailClient.Domain;
using System.Waf.UnitTesting;

namespace EmailClient.Applications.Test
{
    [TestClass]
    public class Pop3SettingsViewModelTest
    {
        public TestContext TestContext { get; set; }


        [TestMethod]
        public void UseSameUserCreditsTest()
        {
            // Initialize the Pop3SettingsViewModel
            Pop3Settings pop3Settings = new Pop3Settings();
            Pop3SettingsViewMock viewMock = new Pop3SettingsViewMock();
            Pop3SettingsViewModel viewModel = new Pop3SettingsViewModel(viewMock, pop3Settings);

            // Set the userName and password for pop3 and smtp account
            viewModel.Pop3UserName = "pop3.userName";
            viewModel.Pop3Password = "pop3.password";
            viewModel.SmtpUserName = "smtp.userName";
            viewModel.SmtpPassword = "smtp.password";

            // Check that the values set in the ViewModel are stored into the domain object
            Assert.AreEqual("smtp.userName", pop3Settings.SmtpUserCredits.UserName);
            Assert.AreEqual("smtp.password", pop3Settings.SmtpUserCredits.Password);

            // Activate the UseSameUserCredits feature
            viewModel.UseSameUserCredits = true;

            // Now the domain object must have the userName and password from the pop3 account
            Assert.AreEqual("pop3.userName", pop3Settings.SmtpUserCredits.UserName);
            Assert.AreEqual("pop3.password", pop3Settings.SmtpUserCredits.Password);

            // Deactivate the UseSameUserCredits feature
            viewModel.UseSameUserCredits = false;

            // The last used userName and password is restored in the domain object
            Assert.AreEqual("smtp.userName", pop3Settings.SmtpUserCredits.UserName);
            Assert.AreEqual("smtp.password", pop3Settings.SmtpUserCredits.Password);
        }


        [TestMethod]
        public void PropertyChangedRelayTest()
        {
            // Initialize the Pop3SettingsViewModel
            Pop3Settings pop3Settings = new Pop3Settings();
            Pop3SettingsViewMock viewMock = new Pop3SettingsViewMock();
            Pop3SettingsViewModel viewModel = new Pop3SettingsViewModel(viewMock, pop3Settings);

            // Set the userName and password property on the domain object and check that it is relayed by the ViewModel
            // with an own PropertyChanged event
            AssertHelper.PropertyChangedEvent(viewModel, x => x.Pop3UserName, () =>
                pop3Settings.Pop3UserCredits.UserName = "pop3.userName");

            AssertHelper.PropertyChangedEvent(viewModel, x => x.Pop3Password, () =>
                pop3Settings.Pop3UserCredits.Password = "pop3.password");
        }
    }
}
