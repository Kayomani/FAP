using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Waf.Applications;
using BookLibrary.Applications.Properties;
using BookLibrary.Applications.Services;
using BookLibrary.Applications.ViewModels;
using System;

namespace BookLibrary.Applications.Controllers
{
    [Export]
    public class ApplicationController : Controller
    {
        private readonly IQuestionService questionService;
        private readonly IEntityController entityController;
        private readonly BookController bookController;
        private readonly PersonController personController;
        private readonly ShellViewModel shellViewModel;
        private readonly DelegateCommand exitCommand;
        

        [ImportingConstructor]
        public ApplicationController(IQuestionService questionService, IPresentationController presentationController, 
            IEntityController entityController, BookController bookController, PersonController personController, 
            ShellViewModel shellViewModel)
        {
            if (presentationController == null) { throw new ArgumentNullException("presentationController"); }
            if (shellViewModel == null) { throw new ArgumentNullException("shellViewModel"); }
            
            presentationController.InitializeCultures();
            
            this.questionService = questionService;
            this.entityController = entityController;
            this.bookController = bookController;
            this.personController = personController;
            this.shellViewModel = shellViewModel;

            this.shellViewModel.Closing += ShellViewModelClosing;
            this.exitCommand = new DelegateCommand(Close);
        }


        public void Initialize()
        {
            shellViewModel.ExitCommand = exitCommand;

            entityController.Initialize();
            bookController.Initialize();
            personController.Initialize();
        }

        public void Run()
        {
            shellViewModel.Show();
        }

        public void Shutdown()
        {
            entityController.Shutdown();
        }

        private void ShellViewModelClosing(object sender, CancelEventArgs e)
        {
            if (entityController.HasChanges)
            {
                if (shellViewModel.IsValid)
                {
                    bool? result = questionService.ShowQuestion(Resources.SaveChangesQuestion);
                    if (result == true)
                    {
                        if (!entityController.Save())
                        {
                            e.Cancel = true;
                        }
                    }
                    else if (result == null)
                    {
                        e.Cancel = true;
                    }
                }
                else
                {
                    e.Cancel = !questionService.ShowYesNoQuestion(Resources.LoseChangesQuestion);
                }
            }
        }

        private void Close()
        {
            shellViewModel.Close();
        }
    }
}
