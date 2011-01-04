using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Waf.Foundation;
using System.ComponentModel.Composition;
using System.IO;
using System.Waf.Applications.Services;

namespace Writer.Applications.Documents
{
    [Export(typeof(IDocumentManager))]
    public class DocumentManager : Model, IDocumentManager
    {
        private readonly IFileDialogService fileDialogService;
        private readonly ObservableCollection<IDocumentType> documentTypes;
        private readonly ReadOnlyObservableCollection<IDocumentType> readOnlyDocumentTypes;
        private readonly ObservableCollection<IDocument> documents;
        private readonly ReadOnlyObservableCollection<IDocument> readOnlyDocuments;
        private IDocument activeDocument;


        [ImportingConstructor]
        public DocumentManager(IFileDialogService fileDialogService)
        {
            if (fileDialogService == null) { throw new ArgumentNullException("fileDialogService"); }

            this.fileDialogService = fileDialogService;
            this.documentTypes = new ObservableCollection<IDocumentType>();
            this.readOnlyDocumentTypes = new ReadOnlyObservableCollection<IDocumentType>(documentTypes);
            this.documents = new ObservableCollection<IDocument>();
            this.readOnlyDocuments = new ReadOnlyObservableCollection<IDocument>(documents);
        }


        public ReadOnlyObservableCollection<IDocumentType> DocumentTypes { get { return readOnlyDocumentTypes; } }

        public ReadOnlyObservableCollection<IDocument> Documents { get { return readOnlyDocuments; } }

        public IDocument ActiveDocument
        {
            get { return activeDocument; }
            set 
            {
                if (activeDocument != value)
                {
                    if (value != null && !documents.Contains(value)) 
                    { 
                        throw new ArgumentException("value is not an item of the Documents collection."); 
                    }
                    activeDocument = value;
                    RaisePropertyChanged("ActiveDocument");
                }
            }
        }


        public event EventHandler<DocumentsClosingEventArgs> DocumentsClosing;


        public void Register(IDocumentType documentType)
        {
            if (documentType == null) { throw new ArgumentNullException("documentType"); }
            documentTypes.Add(documentType);
        }

        public void Deregister(IDocumentType documentType)
        {
            if (documentType == null) { throw new ArgumentNullException("documentType"); }
            if (documents.Any(d => d.DocumentType == documentType)) 
            { 
                throw new InvalidOperationException("It's not possible to deregister a document type which is still used by some documents."); 
            }
            documentTypes.Remove(documentType);
        }

        public IDocument New(IDocumentType documentType)
        {
            if (documentType == null) { throw new ArgumentNullException("documentType"); }
            if (!documentTypes.Contains(documentType))
            {
                throw new ArgumentException("documentType is not an item of the DocumentTypes collection.");
            }
            IDocument document = documentType.New();
            documents.Add(document);
            ActiveDocument = document;
            return document;
        }

        public IDocument Open()
        {
            IEnumerable<FileType> fileTypes = from d in documentTypes
                                              where d.CanOpen()
                                              select new FileType(d.Description, d.FileExtension);
            if (!fileTypes.Any()) { throw new InvalidOperationException("No DocumentType is registered that supports the Open operation."); }

            FileDialogResult result = fileDialogService.ShowOpenFileDialog(fileTypes);
            if (result.IsValid)
            {
                // Check if document is already opened
                IDocument document = documents.SingleOrDefault(d => d.FileName == result.FileName);
                if (document == null)
                {
                    IDocumentType documentType = GetDocumentType(result.SelectedFileType);
                    document = documentType.Open(result.FileName);
                    documents.Add(document);
                }
                ActiveDocument = document;
                return document;
            }
            return null;
        }

        public void Save(IDocument document)
        {
            if (document == null) { throw new ArgumentNullException("document"); }
            if (!documents.Contains(document))
            {
                throw new ArgumentException("document is not an item of the Documents collection.");
            }

            if (Path.IsPathRooted(document.FileName))
            {
                IEnumerable<IDocumentType> saveTypes = documentTypes.Where(d => d.CanSave(document));
                IDocumentType documentType = saveTypes.First(d => d.FileExtension == Path.GetExtension(document.FileName));
                documentType.Save(document, document.FileName);
            }
            else
            {
                SaveAs(document);
            }
        }

        public void SaveAs(IDocument document)
        {
            if (document == null) { throw new ArgumentNullException("document"); }
            if (!documents.Contains(document))
            {
                throw new ArgumentException("document is not an item of the Documents collection.");
            }

            IEnumerable<FileType> fileTypes = from d in documentTypes
                                              where d.CanSave(document)
                                              select new FileType(d.Description, d.FileExtension);
            if (!fileTypes.Any()) { throw new InvalidOperationException("No DocumentType is registered that supports the Save operation."); }

            FileType selectedFileType;
            if (File.Exists(document.FileName))
            {
                IEnumerable<IDocumentType> saveTypes = documentTypes.Where(d => d.CanSave(document));
                IDocumentType documentType = saveTypes.First(d => d.FileExtension == Path.GetExtension(document.FileName));
                selectedFileType = fileTypes.Where(
                    f => f.Description == documentType.Description && f.FileExtension == documentType.FileExtension).First();
            }
            else
            {
                selectedFileType = fileTypes.First();
            }
            string fileName = Path.GetFileNameWithoutExtension(document.FileName);

            FileDialogResult result = fileDialogService.ShowSaveFileDialog(fileTypes, selectedFileType, fileName);
            if (result.IsValid)
            {
                IDocumentType documentType = GetDocumentType(result.SelectedFileType);
                documentType.Save(document, result.FileName);
            }
        }

        public bool Close(IDocument document)
        {
            if (document == null) { throw new ArgumentNullException("document"); }
            if (!documents.Contains(document))
            {
                throw new ArgumentException("document is not an item of the Documents collection.");
            }

            DocumentsClosingEventArgs eventArgs = new DocumentsClosingEventArgs(new IDocument[] { document });
            OnDocumentsClosing(eventArgs);
            if (eventArgs.Cancel) { return false; }

            if (ActiveDocument == document)
            {
                ActiveDocument = null;
            }
            documents.Remove(document);
            return true;
        }

        public bool CloseAll()
        {
            DocumentsClosingEventArgs eventArgs = new DocumentsClosingEventArgs(Documents);
            OnDocumentsClosing(eventArgs);
            if (eventArgs.Cancel) { return false; }

            ActiveDocument = null;
            while (documents.Any())
            {
                documents.Remove(documents.First());
            }
            return true;
        }

        protected virtual void OnDocumentsClosing(DocumentsClosingEventArgs e)
        {
            if (DocumentsClosing != null) { DocumentsClosing(this, e); }
        }

        private IDocumentType GetDocumentType(FileType fileType)
        {
            IDocumentType documentType = (from d in documentTypes
                                          where d.Description == fileType.Description
                                              && d.FileExtension == fileType.FileExtension
                                          select d).First();
            return documentType;
        }
    }
}
