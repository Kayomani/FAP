using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;
using Fap.Application.Views;
using System.Windows.Input;
using Fap.Foundation;
using Fap.Domain.Entity;

namespace Fap.Application.ViewModels
{
    public class CompareViewModel : ViewModel<ICompareView>
    {
        private ICommand run;
        private string status;
        private SafeObservable<CompareNode> data;
        private bool enableStart = true;

        public CompareViewModel(ICompareView view)
            : base(view)
        {

        }

        public bool EnableRun
        {
            get { return enableStart; }
            set
            {
                enableStart = value;
                RaisePropertyChanged("Run");
            }
        }

        public ICommand Run
        {
            get { return run; }
            set
            {
                run = value;
                RaisePropertyChanged("Run");
            }
        }

        public SafeObservable<CompareNode> Data
        {
            get { return data; }
            set
            {
                data = value;
                RaisePropertyChanged("Data");
            }
        }

        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                RaisePropertyChanged("Status");
            }
        }
    }
}
