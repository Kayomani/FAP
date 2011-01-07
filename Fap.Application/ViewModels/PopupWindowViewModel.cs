#region Copyright Kayomani 2010.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
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
using Fap.Application.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Fap.Application.ViewModels
{
    public class PopupWindowViewModel : ViewModel<IPopupWindow>
    {
        private object activeDocumentView;
        private readonly ObservableCollection<PopUpWindowTab> documentViews;
        private ICommand close;
        private ICommand tabClose;


        public ObservableCollection<PopUpWindowTab> DocumentViews { get { return documentViews; } }

        public PopupWindowViewModel(IPopupWindow view)
            : base(view)
        {
            documentViews = new ObservableCollection<PopUpWindowTab>();
        }

        public object ActiveDocumentView
        {
            get { return activeDocumentView; }
            set
            {
                activeDocumentView = value;
                RaisePropertyChanged("ActiveDocumentView");
            }
        }

        public ICommand TabClose
        {
            get { return tabClose; }
            set
            {
                tabClose = value;
                RaisePropertyChanged("TabClose");
            }
        }

        public ICommand Close
        {
            get { return close; }
            set
            {
                close = value;
                RaisePropertyChanged("Close");
            }
        }

        public void Show()
        {
            ViewCore.Show();
        }

        public void CloseWindow()
        {
            ViewCore.Close();
        }

        public void FlashIfNotActive()
        {
            ViewCore.FlashIfNotActive();
        }
    }
}
