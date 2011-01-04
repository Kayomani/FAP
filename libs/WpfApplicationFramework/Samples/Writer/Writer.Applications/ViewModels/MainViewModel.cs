using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;
using Writer.Applications.Views;
using Writer.Applications.Documents;
using System.Waf.Applications.Services;
using Writer.Applications.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Globalization;
using System.ComponentModel.Composition;
using System.ComponentModel;
using Writer.Applications.Properties;

namespace Writer.Applications.ViewModels
{
    [Export]
    public class MainViewModel : ViewModel<IMainView>
    {
        private readonly IDocumentManager documentManager;
        private readonly IMessageService messageService;
        private readonly ObservableCollection<object> documentViews;
        private readonly DelegateCommand newCommand;
        private readonly DelegateCommand openCommand;
        private readonly DelegateCommand closeCommand;
        private readonly DelegateCommand saveCommand;
        private readonly DelegateCommand saveAsCommand;
        private readonly DelegateCommand englishCommand;
        private readonly DelegateCommand germanCommand;
        private readonly DelegateCommand aboutCommand;
        private readonly DelegateCommand nextDocumentCommand;
        private ICommand printPreviewCommand;
        private ICommand printCommand;
        private ICommand exitCommand;
        private IDocument activeDocument;
        private object activeDocumentView;
        private CultureInfo newLanguage;


        [ImportingConstructor]
        public MainViewModel(IMainView view, IDocumentManager documentManager, IMessageService messageService) 
            : base(view)
        {
            this.documentManager = documentManager;
            this.messageService = messageService;
            this.documentViews = new ObservableCollection<object>();
            this.newCommand = new DelegateCommand(NewDocument);
            this.openCommand = new DelegateCommand(OpenDocument);
            this.closeCommand = new DelegateCommand(CloseDocument, CanCloseDocument);
            this.saveCommand = new DelegateCommand(SaveDocument, CanSaveDocument);
            this.saveAsCommand = new DelegateCommand(SaveAsDocument, CanSaveAsDocument);
            this.englishCommand = new DelegateCommand(() => SelectLanguage(new CultureInfo("en-US")));
            this.germanCommand = new DelegateCommand(() => SelectLanguage(new CultureInfo("de-DE")));
            this.aboutCommand = new DelegateCommand(ShowAboutMessage);
            this.nextDocumentCommand = new DelegateCommand(ActivateNextDocument);

            AddWeakEventListener(documentManager, DocumentManagerPropertyChanged);
        }


        public ObservableCollection<object> DocumentViews { get { return documentViews; } }

        public object ActiveDocumentView
        {
            get { return activeDocumentView; }
            set
            {
                if (activeDocumentView != value)
                {
                    activeDocumentView = value;
                    RaisePropertyChanged("ActiveDocumentView");
                }
            }
        }

        public CultureInfo NewLanguage { get { return newLanguage; } }

        public ICommand NewCommand { get { return newCommand; } }

        public ICommand OpenCommand { get { return openCommand; } }

        public ICommand CloseCommand { get { return closeCommand; } }

        public ICommand SaveCommand { get { return saveCommand; } }

        public ICommand SaveAsCommand { get { return saveAsCommand; } }

        public ICommand PrintPreviewCommand
        {
            get { return printPreviewCommand; }
            set
            {
                if (printPreviewCommand != value)
                {
                    printPreviewCommand = value;
                    RaisePropertyChanged("PrintPreviewCommand");
                }
            }
        }

        public ICommand PrintCommand 
        { 
            get { return printCommand; }
            set
            {
                if (printCommand != value)
                {
                    printCommand = value;
                    RaisePropertyChanged("PrintCommand");
                }
            }
        }

        public ICommand ExitCommand
        {
            get { return exitCommand; }
            set
            {
                if (exitCommand != value)
                {
                    exitCommand = value;
                    RaisePropertyChanged("ExitCommand");
                }
            }
        }

        public ICommand EnglishCommand { get { return englishCommand; } }

        public ICommand GermanCommand { get { return germanCommand; } }

        public ICommand AboutCommand { get { return aboutCommand; } }

        public ICommand NextDocumentCommand { get { return nextDocumentCommand; } }


        private void NewDocument()
        {
            documentManager.New(documentManager.DocumentTypes.First());
        }

        private void OpenDocument()
        {
            documentManager.Open();
        }

        private bool CanCloseDocument()
        {
            return documentManager.ActiveDocument != null;
        }
        
        private void CloseDocument()
        {
            documentManager.Close(documentManager.ActiveDocument);
        }

        private bool CanSaveDocument()
        {
            return documentManager.ActiveDocument != null && documentManager.ActiveDocument.Modified;
        }

        private void SaveDocument()
        {
            documentManager.Save(documentManager.ActiveDocument);
        }

        private bool CanSaveAsDocument()
        {
            return documentManager.ActiveDocument != null;
        }

        private void SaveAsDocument()
        {
            documentManager.SaveAs(documentManager.ActiveDocument);
        }

        private void SelectLanguage(CultureInfo uiCulture)
        {
            if (!uiCulture.Equals(CultureInfo.CurrentUICulture))
            {
                messageService.ShowMessage(Resources.RestartApplication + "\n\n" +
                    Resources.ResourceManager.GetString("RestartApplication", uiCulture));
            }
            newLanguage = uiCulture;
        }

        private void ShowAboutMessage()
        {
            messageService.ShowMessage(string.Format(CultureInfo.CurrentCulture, Resources.AboutText, 
                ApplicationInfo.ProductName, ApplicationInfo.Version));
        }

        private void ActivateNextDocument()
        {
            if (documentManager.Documents.Count > 1)
            {
                int index = documentManager.Documents.IndexOf(documentManager.ActiveDocument) + 1;
                if (index >= documentManager.Documents.Count)
                {
                    index = 0;
                }
                documentManager.ActiveDocument = documentManager.Documents[index];
            }
        }

        private void DocumentManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveDocument")
            {
                if (activeDocument != null) { RemoveWeakEventListener(activeDocument, ActiveDocumentPropertyChanged); }

                activeDocument = documentManager.ActiveDocument;

                if (activeDocument != null) { AddWeakEventListener(activeDocument, ActiveDocumentPropertyChanged); }

                UpdateCommands();
            }
        }

        private void ActiveDocumentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Modified")
            {
                UpdateCommands();
            }
        }

        private void UpdateCommands()
        {
            closeCommand.RaiseCanExecuteChanged();
            saveCommand.RaiseCanExecuteChanged();
            saveAsCommand.RaiseCanExecuteChanged();
        }
    }
}
