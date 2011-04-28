using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;
using FAP.Application.Views;
using System.ComponentModel;
using System.Windows.Input;
using System.Net.NetworkInformation;

namespace FAP.Application.ViewModels
{
    public class InterfaceSelectionViewModel: ViewModel<IInterfaceSelectionView>
    {
        private BindingList<NetworkInterface> interfaces;
        private ICommand quit;
        private ICommand select;
        private NetworkInterface selectedInterface;
        
        public InterfaceSelectionViewModel(IInterfaceSelectionView i): base(i){}

        public BindingList<NetworkInterface> Interfaces
        {
            set { interfaces = value; RaisePropertyChanged("Interfaces"); }
            get { return interfaces; }
        }

        public NetworkInterface SelectedInterface
        {
            set { selectedInterface = value; RaisePropertyChanged("SelectedInterface"); }
            get { return selectedInterface; }
        }

        public ICommand Quit
        {
            set { quit = value; RaisePropertyChanged("Quit"); }
            get { return quit; }
        }

        public ICommand Select
        {
            set { select = value; RaisePropertyChanged("Select"); }
            get { return select; }
        }

        public void ShowDialog()
        {
            ViewCore.ShowDialog();
        }
        public void Close()
        {
            ViewCore.Close();
        }
    }
}
