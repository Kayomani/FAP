using System.ComponentModel.Composition.Hosting;
using System.Waf.Applications;
using BookLibrary.Applications.Controllers;
using BookLibrary.Applications.Test.Services;
using BookLibrary.Applications.Test.Views;
using BookLibrary.Applications.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BookLibrary.Applications.Properties;
using System.Waf.UnitTesting;
using System;

namespace BookLibrary.Applications.Test.Controllers
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
        }


        [TestMethod]
        public void ApplicationControllerConstructorTest()
        {
            AssertHelper.ExpectedException<ArgumentNullException>(() =>
                new ApplicationController(null, null, null, null, null, null));
            AssertHelper.ExpectedException<ArgumentNullException>(() =>
                            new ApplicationController(null, new PresentationControllerMock(), null, null, null, null));
        }
        
        [TestMethod]
        public void ApplicationControllerLifecycleTest()
        {
            PresentationControllerMock presentationController = container.GetExportedValue<PresentationControllerMock>();
            EntityControllerMock entityController = container.GetExportedValue<EntityControllerMock>();
            ApplicationController applicationController = container.GetExportedValue<ApplicationController>();
            Assert.IsTrue(presentationController.InitializeCulturesCalled);

            // Initialize
            Assert.IsFalse(entityController.InitializeCalled);
            applicationController.Initialize();
            Assert.IsTrue(entityController.InitializeCalled);

            // Run
            ShellViewMock shellView = container.GetExportedValue<ShellViewMock>();
            Assert.IsFalse(shellView.IsVisible);
            applicationController.Run();
            Assert.IsTrue(shellView.IsVisible);

            // Exit the ShellView
            ShellViewModel shellViewModel = shellView.GetViewModel<ShellViewModel>();
            shellViewModel.ExitCommand.Execute(null);
            Assert.IsFalse(shellView.IsVisible);

            // Shutdown
            Assert.IsFalse(entityController.ShutdownCalled);
            applicationController.Shutdown();
            Assert.IsTrue(entityController.ShutdownCalled);
        }

        [TestMethod]
        public void ApplicationControllerHasChangesTest()
        {
            QuestionServiceMock questionService = container.GetExportedValue<QuestionServiceMock>();
            EntityControllerMock entityController = container.GetExportedValue<EntityControllerMock>();
            ApplicationController applicationController = container.GetExportedValue<ApplicationController>();

            applicationController.Initialize();
            applicationController.Run();

            ShellViewMock shellView = container.GetExportedValue<ShellViewMock>();
            ShellViewModel shellViewModel = shellView.GetViewModel<ShellViewModel>();


            // Exit the application although we have unsaved changes.
            entityController.HasChangesResult = true;
            // When the question box asks us to save the changes we say "Yes" => true.
            questionService.ShowQuestionAction = (message) =>
            {
                Assert.AreEqual(Resources.SaveChangesQuestion, message);
                return true;
            };
            // Then we simulate that the EntityController wasn't able to save the changes.
            entityController.SaveResult = false;
            shellViewModel.ExitCommand.Execute(null);
            // The Save method must be called. Because the save operation failed the expect the ShellView to be
            // still visible.
            Assert.IsTrue(entityController.SaveCalled);
            Assert.IsTrue(shellView.IsVisible);


            // Exit the application although we have unsaved changes.
            entityController.HasChangesResult = true;
            entityController.SaveCalled = false;
            // When the question box asks us to save the changes we say "Cancel" => null.
            questionService.ShowQuestionAction = (message) => null;
            // This time the Save method must not be called. Because we have chosen "Cancel" the ShellView must still
            // be visible.
            shellViewModel.ExitCommand.Execute(null);
            Assert.IsFalse(entityController.SaveCalled);
            Assert.IsTrue(shellView.IsVisible);


            // Exit the application although we have unsaved changes.
            entityController.HasChangesResult = true;
            entityController.SaveCalled = false;
            // When the question box asks us to save the changes we say "No" => false.
            questionService.ShowQuestionAction = (message) => false;
            // This time the Save method must not be called. Because we have chosen "No" the ShellView must still
            // be closed.
            shellViewModel.ExitCommand.Execute(null);
            Assert.IsFalse(entityController.SaveCalled);
            Assert.IsFalse(shellView.IsVisible);
        }

        [TestMethod]
        public void ApplicationControllerIsInvalidTest()
        {
            QuestionServiceMock questionService = container.GetExportedValue<QuestionServiceMock>();
            EntityControllerMock entityController = container.GetExportedValue<EntityControllerMock>();
            ApplicationController applicationController = container.GetExportedValue<ApplicationController>();

            applicationController.Initialize();
            applicationController.Run();

            ShellViewMock shellView = container.GetExportedValue<ShellViewMock>();
            ShellViewModel shellViewModel = shellView.GetViewModel<ShellViewModel>();


            // Exit the application although we have unsaved changes.
            entityController.HasChangesResult = true;
            // Simulate UI errors on the ShellView.
            shellViewModel.IsValid = false;
            // When the question box asks us to loose our changes we say "No" => false.
            questionService.ShowYesNoQuestionAction = (message) =>
            {
                Assert.AreEqual(Resources.LoseChangesQuestion, message);
                return false;
            };
            shellViewModel.ExitCommand.Execute(null);
            // We expect the ShellView to stay open.
            Assert.IsTrue(shellView.IsVisible);

            
            // Exit the application again but this time we agree to loose our changes.
            questionService.ShowYesNoQuestionAction = (message) => true;
            shellViewModel.ExitCommand.Execute(null);
            Assert.IsFalse(shellView.IsVisible);
        }
    }
}
