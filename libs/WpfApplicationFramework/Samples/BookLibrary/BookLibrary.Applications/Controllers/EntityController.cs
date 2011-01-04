using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data;
using System.Globalization;
using System.Waf.Applications;
using System.Waf.Applications.Services;
using BookLibrary.Applications.Properties;
using BookLibrary.Applications.Services;
using BookLibrary.Applications.ViewModels;
using BookLibrary.Domain;
using System.ComponentModel.DataAnnotations;

namespace BookLibrary.Applications.Controllers
{
    /// <summary>
    /// This controller is responsible for the database connection and the save operation.
    /// </summary>
    [Export(typeof(IEntityController))]
    internal class EntityController : Controller, IEntityController
    {
        private readonly EntityService entityService;
        private readonly IMessageService messageService;
        private readonly ShellViewModel shellViewModel;
        private readonly DelegateCommand saveCommand;
        private BookLibraryEntities entities;


        [ImportingConstructor]
        public EntityController(EntityService entityService, IMessageService messageService, ShellViewModel mainViewModel)
        {
            this.entityService = entityService;
            this.messageService = messageService;
            this.shellViewModel = mainViewModel;
            this.saveCommand = new DelegateCommand(() => Save(), CanSave);
        }


        public bool HasChanges
        {
            get { return entities != null && entities.HasChanges; }
        }


        public void Initialize()
        {
            entities = new BookLibraryEntities();
            entityService.Entities = entities;

            shellViewModel.PropertyChanged += ShellViewModelPropertyChanged;
            shellViewModel.SaveCommand = saveCommand;
        }

        public void Shutdown()
        {
            entities.Dispose();
        }

        public bool CanSave() { return shellViewModel.IsValid; }
        
        public bool Save()
        {
            bool saved = false;
            if (!CanSave()) 
            { 
                throw new InvalidOperationException("You must not call Save when CanSave returns false."); 
            }
            try
            {
                entities.SaveChanges();
                saved = true;
            }
            catch (ValidationException e)
            {
                messageService.ShowError(string.Format(CultureInfo.CurrentCulture, Resources.SaveErrorInvalidEntities, 
                    e.Message));
            }
            catch (UpdateException e)
            {
                messageService.ShowError(string.Format(CultureInfo.CurrentCulture, Resources.SaveErrorInvalidFields,
                    e.InnerException.Message));
            }
            return saved;
        }

        private void ShellViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsValid")
            {
                saveCommand.RaiseCanExecuteChanged();
            }
        }
    }
}
