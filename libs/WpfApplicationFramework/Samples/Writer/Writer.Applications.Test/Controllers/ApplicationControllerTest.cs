using System;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.Linq;
using System.Waf.Applications;
using System.Waf.Applications.Services;
using System.Waf.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Writer.Applications.Controllers;
using Writer.Applications.Documents;
using Writer.Applications.Properties;
using Writer.Applications.Test.Services;
using Writer.Applications.Test.Views;
using Writer.Applications.ViewModels;
using Writer.Applications.Views;

namespace Writer.Applications.Test.Controllers
{
    [TestClass]
    public class ApplicationControllerTest
    {
        private TestController controller;
        private CompositionContainer container;


        [TestInitialize]
        public void TestInitialize()
        {
            controller = new TestController();
            container = controller.Container;
            controller.InitializeRichTextDocumentSupport();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            SaveChangesViewMock.ShowDialogAction = null;
        }


        [TestMethod]
        public void ApplicationControllerConstructorTest()
        {
            AssertHelper.ExpectedException<ArgumentNullException>(() => 
                new ApplicationController(null, null, null));
            AssertHelper.ExpectedException<ArgumentNullException>(() => 
                new ApplicationController(container, null, null));
            AssertHelper.ExpectedException<ArgumentNullException>(() => 
                new ApplicationController(container, new PresentationControllerMock(), null));
        }
        
        [TestMethod]
        public void ControllerLifecycle()
        {
            ApplicationController applicationController = container.GetExportedValue<ApplicationController>();
            
            applicationController.Initialize();
            IDocumentManager documentManager = container.GetExportedValue<IDocumentManager>();
            Assert.IsInstanceOfType(documentManager.DocumentTypes.First(), typeof(RichTextDocumentType));
            Assert.IsInstanceOfType(documentManager.DocumentTypes.Last(), typeof(XpsExportDocumentType));
            MainViewModel mainViewModel = container.GetExportedValue<MainViewModel>();
            Assert.IsNotNull(mainViewModel.ExitCommand);

            applicationController.Run();
            Assert.IsInstanceOfType(documentManager.Documents.Single(), typeof(RichTextDocument));
            ShellViewMock shellView = (ShellViewMock)container.GetExportedValue<IShellView>();
            Assert.IsTrue(shellView.IsVisible);

            mainViewModel.ExitCommand.Execute(null);
            Assert.IsFalse(shellView.IsVisible);

            applicationController.Shutdown();
        }


        [TestMethod]
        public void SaveChangesTest()
        {
            ApplicationController applicationController = container.GetExportedValue<ApplicationController>();
            applicationController.Initialize();
            applicationController.Run();

            MainViewModel mainViewModel = container.GetExportedValue<MainViewModel>();
            RichTextViewModel richTextViewModel = ((IView)mainViewModel.ActiveDocumentView).GetViewModel<RichTextViewModel>();
            richTextViewModel.Document.Modified = true;

            bool showDialogCalled = false;
            SaveChangesViewMock.ShowDialogAction = (view) =>
            {
                showDialogCalled = true;
                Assert.IsTrue(view.GetViewModel<SaveChangesViewModel>().Documents.SequenceEqual(
                    new[] { richTextViewModel.Document }));
                view.Close();
            };

            // When we try to close the ShellView then the appController shows the SaveChangesView because the
            // modified document wasn't saved.
            mainViewModel.ExitCommand.Execute(null);
            Assert.IsTrue(showDialogCalled);
            ShellViewMock shellView = (ShellViewMock)container.GetExportedValue<IShellView>();
            Assert.IsTrue(shellView.IsVisible);

            showDialogCalled = false;
            SaveChangesViewMock.ShowDialogAction = (view) =>
            {
                showDialogCalled = true;
                view.ViewModel.YesCommand.Execute(null);
            };

            FileDialogServiceMock fileDialogService = (FileDialogServiceMock)container.GetExportedValue<IFileDialogService>();
            fileDialogService.Result = new FileDialogResult();

            // This time we let the SaveChangesView to save the modified document
            mainViewModel.ExitCommand.Execute(null);
            Assert.IsTrue(showDialogCalled);
            Assert.AreEqual(FileDialogType.SaveFileDialog, fileDialogService.FileDialogType);
            Assert.IsFalse(shellView.IsVisible);
        }

        [TestMethod]
        public void SettingsTest()
        {
            Settings.Default.Culture = "de-DE";
            Settings.Default.UICulture = "de-AT";

            ApplicationController applicationController = container.GetExportedValue<ApplicationController>();
            
            Assert.AreEqual(new CultureInfo("de-DE"), CultureInfo.CurrentCulture);
            Assert.AreEqual(new CultureInfo("de-AT"), CultureInfo.CurrentUICulture);

            applicationController.Initialize();
            applicationController.Run();

            MainViewModel mainViewModel = container.GetExportedValue<MainViewModel>();
            mainViewModel.EnglishCommand.Execute(null);
            Assert.AreEqual(new CultureInfo("en-US"), mainViewModel.NewLanguage);

            bool settingsSaved = false;
            Settings.Default.SettingsSaving += (sender, e) =>
            {
                settingsSaved = true;
            };

            applicationController.Shutdown();
            Assert.AreEqual("en-US", Settings.Default.UICulture);
            Assert.IsTrue(settingsSaved);
        }
    }
}
