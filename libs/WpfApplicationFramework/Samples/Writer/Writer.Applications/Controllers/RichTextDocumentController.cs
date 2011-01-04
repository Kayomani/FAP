using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Waf.Applications;
using Writer.Applications.Documents;
using Writer.Applications.ViewModels;
using Writer.Applications.Views;

namespace Writer.Applications.Controllers
{
    [Export]
    public class RichTextDocumentController : DocumentController
    {
        private readonly CompositionContainer container;
        private readonly IDocumentManager documentManager;
        private readonly MainViewModel mainViewModel;
        private readonly Dictionary<RichTextDocument, RichTextViewModel> richTextViewModels;

        
        [ImportingConstructor]
        public RichTextDocumentController(CompositionContainer container, IDocumentManager documentManager, MainViewModel mainViewModel) 
            : base(documentManager)
        {
            this.container = container;
            this.documentManager = documentManager;
            this.mainViewModel = mainViewModel;
            this.richTextViewModels = new Dictionary<RichTextDocument, RichTextViewModel>();
            this.mainViewModel.PropertyChanged += MainViewModelPropertyChanged;
        }

        
        protected override void OnDocumentAdded(IDocument document)
        {
            RichTextDocument richTextDocument = document as RichTextDocument;
            if (richTextDocument != null)
            {
                IRichTextView richTextView = container.GetExportedValue<IRichTextView>();
                RichTextViewModel richTextViewModel = new RichTextViewModel(richTextView, richTextDocument);
                richTextViewModels.Add(richTextDocument, richTextViewModel);
                mainViewModel.DocumentViews.Add(richTextViewModel.View);
            }
        }

        protected override void OnDocumentRemoved(IDocument document)
        {
            RichTextDocument richTextDocument = document as RichTextDocument;
            if (richTextDocument != null)
            {
                mainViewModel.DocumentViews.Remove(richTextViewModels[richTextDocument].View);
                richTextViewModels.Remove(richTextDocument);
            }
        }

        protected override void OnActiveDocumentChanged(IDocument activeDocument)
        {
            if (activeDocument == null)
            {
                mainViewModel.ActiveDocumentView = null;
            }
            else
            {
                RichTextDocument richTextDocument = activeDocument as RichTextDocument;
                if (richTextDocument != null)
                {
                    mainViewModel.ActiveDocumentView = richTextViewModels[richTextDocument].View;
                }
            }
        }

        private void MainViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveDocumentView")
            {
                IView richTextView = mainViewModel.ActiveDocumentView as IView;
                if (richTextView != null)
                {
                    RichTextViewModel richTextViewModel = richTextView.GetViewModel<RichTextViewModel>();
                    if (richTextViewModel != null)
                    {
                        documentManager.ActiveDocument = richTextViewModel.Document;
                    }
                }
            }
        }
    }
}
