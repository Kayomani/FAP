using System.Linq;
using EmailClient.Applications;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EmailClient.Applications.Views;
using EmailClient.Domain;
using EmailClient.Applications.ViewModels;
using EmailClient.Applications.Test.Mocks;
using System.Waf.UnitTesting;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using EmailClient.Applications.Controllers;

namespace EmailClient.Applications.Test
{
    [TestClass]
    public class CreateEmailAccountControllerTest
    {
        public TestContext TestContext { get; set; }


        [TestMethod]
        public void CreateExchangeAccountTest()
        {
            // Create the IoC Container
            AggregateCatalog catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(ApplicationController).Assembly));
            CompositionContainer container = new CompositionContainer(catalog);
            CompositionBatch batch = new CompositionBatch();
            batch.AddExportedValue(container);

            // Initialize the mock views
            ShellViewMock shellViewMock = new ShellViewMock();
            EmailAccountsViewMock emailAccountsViewMock = new EmailAccountsViewMock();
            CreateEmailAccountViewMock createEmailAccountViewMock = new CreateEmailAccountViewMock();
            ExchangeSettingsViewMock exchangeSettingsViewMock = new ExchangeSettingsViewMock();
            batch.AddExportedValue<IShellView>(shellViewMock);
            batch.AddExportedValue<IMessageListView>(new MessageListViewMock());
            batch.AddExportedValue<IMessageContentView>(new MessageContentViewMock());
            batch.AddExportedValue<IEmailAccountsView>(emailAccountsViewMock);
            batch.AddExportedValue<ICreateEmailAccountView>(createEmailAccountViewMock);
            batch.AddExportedValue<IExchangeSettingsView>(exchangeSettingsViewMock);
            container.Compose(batch);

            // Initialize the application controller and get the domain object
            ApplicationController applicationController = container.GetExportedValue<ApplicationController>();
            applicationController.Initialize();
            PrivateObject privateApplicationController = new PrivateObject(applicationController);
            EmailClientRoot emailClientRoot = (EmailClientRoot)privateApplicationController.GetField("emailClientRoot");

            // The emailClientRoot shouldn't contain any email accounts yet
            Assert.AreEqual(0, emailClientRoot.EmailAccounts.Count());

            // Run the application controller
            applicationController.Run();
            
            // The Shell should be visible now
            Assert.IsTrue(shellViewMock.IsVisible);

            // Execute the Email Accounts command
            shellViewMock.ViewModel.EmailAccountsCommand.Execute(null);

            // The wizard dialog should be visible now
            Assert.IsTrue(emailAccountsViewMock.IsVisible);

            // Select the Exchange account and execute the next command
            createEmailAccountViewMock.ViewModel.IsExchangeChecked = true;
            emailAccountsViewMock.ViewModel.NextCommand.Execute(null);

            // Set the exchange account data and execute the next command
            exchangeSettingsViewMock.ViewModel.Model.ServerPath = "exchange.test.com";
            exchangeSettingsViewMock.ViewModel.Model.UserName = "testUser";
            emailAccountsViewMock.ViewModel.NextCommand.Execute(null);

            // The wizard should be finished now and thus the dialog closed
            Assert.IsFalse(emailAccountsViewMock.IsVisible);

            // Check that the email account is saved in our domain objects
            Assert.AreEqual(1, emailClientRoot.EmailAccounts.Count());
            ExchangeSettings exchangeSettings = (ExchangeSettings)emailClientRoot.EmailAccounts.ElementAt(0);
            Assert.AreEqual("exchange.test.com", exchangeSettings.ServerPath);
            Assert.AreEqual("testUser", exchangeSettings.UserName);

            // Close the Shell
            shellViewMock.ViewModel.ExitCommand.Execute(null);
            Assert.IsFalse(shellViewMock.IsVisible);
        }
    }
}
