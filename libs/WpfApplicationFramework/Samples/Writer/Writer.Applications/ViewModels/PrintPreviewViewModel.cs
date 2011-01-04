using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Writer.Applications.Views;
using System.Waf.Applications;
using System.Windows.Input;
using System.Windows.Documents;

namespace Writer.Applications.ViewModels
{
    public class PrintPreviewViewModel : ViewModel<IPrintPreviewView>
    {
        private readonly IDocumentPaginatorSource document;
        private DelegateCommand printCommand;
        private ICommand closeCommand;
        
        
        public PrintPreviewViewModel(IPrintPreviewView view, IDocumentPaginatorSource document)
            : base(view)
        {
            this.document = document;
            this.printCommand = new DelegateCommand(Print);
        }


        public IDocumentPaginatorSource Document
        {
            get { return document; }
        }

        public ICommand PrintCommand { get { return printCommand; } }

        public ICommand CloseCommand
        {
            get { return closeCommand; }
            set
            {
                if (closeCommand != value)
                {
                    closeCommand = value;
                    RaisePropertyChanged("CloseCommand");
                }
            }
        }


        private void Print()
        {
            ViewCore.Print();
        }
    }
}

