using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Waf.Applications;
using Writer.Applications.Documents;

namespace Writer.Applications.Controllers
{
    /// <summary>
    /// A DocumentController is responsible to synchronize the Documents with the UI Elements that represent these Documents.
    /// </summary>
    public abstract class DocumentController : Controller
    {
        private readonly IDocumentManager documentManager;
        
        
        protected DocumentController(IDocumentManager documentManager)
        {
            if (documentManager == null) { throw new ArgumentNullException("documentManager"); }
            
            this.documentManager = documentManager;
            AddWeakEventListener(documentManager, DocumentManagerPropertyChanged);
            AddWeakEventListener(documentManager.Documents, DocumentsCollectionChanged);
        }


        protected abstract void OnDocumentAdded(IDocument document);

        protected abstract void OnDocumentRemoved(IDocument document);

        protected abstract void OnActiveDocumentChanged(IDocument activeDocument);

        private void DocumentManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveDocument") { OnActiveDocumentChanged(documentManager.ActiveDocument); }
        }

        private void DocumentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnDocumentAdded(e.NewItems.Cast<Document>().Single());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnDocumentRemoved(e.OldItems.Cast<Document>().Single());
                    break;
                default:
                    throw new NotSupportedException("This kind of documents collection change is not supported.");
            }
        }
    }
}
