#region Copyright Kayomani 2011.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
/**
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or any 
    later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications;
using FAP.Application.Views;
using System.ComponentModel;
using System.Windows.Input;
using System.Net.NetworkInformation;
using FAP.Domain.Entities;

namespace FAP.Application.ViewModels
{
    public class InterfaceSelectionViewModel: ViewModel<IInterfaceSelectionView>
    {
        private BindingList<NetInterface> interfaces;
        private ICommand quit;
        private ICommand select;
        private NetInterface selectedInterface;
        
        public InterfaceSelectionViewModel(IInterfaceSelectionView i): base(i){}

        public BindingList<NetInterface> Interfaces
        {
            set { interfaces = value; RaisePropertyChanged("Interfaces"); }
            get { return interfaces; }
        }

        public NetInterface SelectedInterface
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
