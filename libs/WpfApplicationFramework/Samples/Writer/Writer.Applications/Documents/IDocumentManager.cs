using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Writer.Applications.Documents
{
    public interface IDocumentManager : INotifyPropertyChanged
    {
        ReadOnlyObservableCollection<IDocumentType> DocumentTypes { get; }

        ReadOnlyObservableCollection<IDocument> Documents { get; }

        IDocument ActiveDocument { get; set; }


        event EventHandler<DocumentsClosingEventArgs> DocumentsClosing;


        void Register(IDocumentType documentType);

        void Deregister(IDocumentType documentType);

        IDocument New(IDocumentType documentType);

        IDocument Open();

        bool Close(IDocument document);

        bool CloseAll();

        void Save(IDocument document);

        void SaveAs(IDocument document);
    }
}
