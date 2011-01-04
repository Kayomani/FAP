using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;
using Writer.Applications.Views;
using System.ComponentModel.Composition;
using Writer.Applications.Documents;
using System.Windows.Input;
using Writer.Applications.Services;

namespace Writer.Applications.ViewModels
{
    public class SaveChangesViewModel : ViewModel<ISaveChangesView>
    {
        private readonly IZoomService zoomService;
        private readonly IEnumerable<IDocument> documents;
        private readonly DelegateCommand yesCommand;
        private readonly DelegateCommand noCommand;
        private ViewResult viewResult;

        
        public SaveChangesViewModel(ISaveChangesView view, IZoomService zoomService,
            IEnumerable<IDocument> documents) : base(view)
        {
            this.zoomService = zoomService;
            this.documents = documents;
            this.yesCommand = new DelegateCommand(() => Close(ViewResult.Yes));
            this.noCommand = new DelegateCommand(() => Close(ViewResult.No));
        }


        public static string Title { get { return ApplicationInfo.ProductName; } }

        public IZoomService ZoomService { get { return zoomService; } }

        public IEnumerable<IDocument> Documents { get { return documents; } }

        public ViewResult ViewResult { get { return viewResult; } }

        public ICommand YesCommand { get { return yesCommand; } }

        public ICommand NoCommand { get { return noCommand; } }


        public void ShowDialog(object owner)
        {
            viewResult = ViewResult.Cancel;
            ViewCore.ShowDialog(owner);
        }

        private void Close(ViewResult viewResult)
        {
            this.viewResult = viewResult;
            ViewCore.Close();
        }
    }
}
