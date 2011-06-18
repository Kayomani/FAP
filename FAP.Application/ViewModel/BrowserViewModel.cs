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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Waf.Applications;
using System.Windows.Input;
using FAP.Application.Views;
using FAP.Domain.Entities.FileSystem;

namespace FAP.Application.ViewModels
{
    public class BrowserViewModel : ViewModel<IBrowserView>
    {
        private BrowsingFile currentItem;
        private string currentPath;
        private ICommand download;
        private bool isBusy;
        private List<BrowsingFile> lastSelectedEntity;
        private bool noCache;
        private ICommand refresh;
        private ObservableCollection<BrowsingFile> root = new ObservableCollection<BrowsingFile>();
        private string status;

        public BrowserViewModel(IBrowserView view)
            : base(view)
        {
        }

        public bool NoCache
        {
            set
            {
                noCache = value;
                RaisePropertyChanged("NoCache");
            }
            get { return noCache; }
        }

        public bool IsBusy
        {
            set
            {
                isBusy = value;
                RaisePropertyChanged("IsBusy");
            }
            get { return isBusy; }
        }

        public ObservableCollection<BrowsingFile> Root
        {
            set
            {
                root = value;
                RaisePropertyChanged("Root");
            }
            get { return root; }
        }

        public string Status
        {
            set
            {
                status = value;
                RaisePropertyChanged("Status");
            }
            get { return status; }
        }

        public ICommand Download
        {
            set
            {
                download = value;
                RaisePropertyChanged("Download");
            }
            get { return download; }
        }

        public ICommand Refresh
        {
            set
            {
                refresh = value;
                RaisePropertyChanged("Refresh");
            }
            get { return refresh; }
        }

        public List<BrowsingFile> LastSelectedEntity
        {
            get { return lastSelectedEntity; }
            set
            {
                lastSelectedEntity = value;
                RaisePropertyChanged("LastSelectedEntity");
            }
        }

        public BrowsingFile CurrentItem
        {
            get { return currentItem; }
            set
            {
                currentItem = value;
                RaisePropertyChanged("CurrentItem");
            }
        }

        public string CurrentPath
        {
            get { return currentPath; }
            set
            {
                currentPath = value;
                RaisePropertyChanged("CurrentPath");
            }
        }
    }
}