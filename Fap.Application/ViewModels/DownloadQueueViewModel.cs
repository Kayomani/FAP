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
using Fap.Foundation;
using Fap.Domain.Entity;
using System.Windows.Input;
using System.Collections;

namespace Fap.Application.ViewModels
{
    public class DownloadQueueViewModel : ViewModel<IDownloadQueue>
    {
        private SafeObservable<DownloadRequest> downloadQueue;
        private IList selectedItems;

        private ICommand removeAll;
        private ICommand removeSelection;

        public DownloadQueueViewModel(IDownloadQueue view)
            : base(view)
        {
        }

        public ICommand RemoveSelection
        {
            get { return removeSelection; }
            set
            {
                removeSelection = value;
                RaisePropertyChanged("RemoveSelection");
            }
        }

        public ICommand RemoveAll
        {
            get { return removeAll; }
            set
            {
                removeAll = value;
                RaisePropertyChanged("RemoveAll");
            }
        }

        public SafeObservable<DownloadRequest> DownloadQueue
        {
            set { downloadQueue = value; RaisePropertyChanged("DownloadQueue"); }
            get { return downloadQueue; }
        }

        public IList SelectedItems
        {
            set { selectedItems = value; RaisePropertyChanged("SelectedItems"); }
            get { return selectedItems; }
        }
    }
}
