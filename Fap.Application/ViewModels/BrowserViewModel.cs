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
using System.Windows.Controls;
using System.Windows.Input;

namespace Fap.Application.ViewModels
{
    public class BrowserViewModel: ViewModel<IBrowserView>
    {
        private FileSystemEntity root = new FileSystemEntity();
        private FileSystemEntity currentItem;
        private string currentPath;

        private string status;
        private ICommand download;
        private ICommand refresh;
        private ICommand browseFolder;
        private List<FileSystemEntity> lastSelectedEntity;
        private string name;

        public BrowserViewModel(IBrowserView view)
            : base(view)
        {

        }


        public string Name
        {
            set
            {
                name = value;
                RaisePropertyChanged("Name");
            }
            get
            {
                return name;
            }
        } 

        public FileSystemEntity Root
        {
            set
            {
                root = value;
                RaisePropertyChanged("Root");
            }
            get
            {
                return root;
            }
        }

        public ICommand BrowseFolder
        {
            set
            {
                browseFolder = value;
                RaisePropertyChanged("BrowseFolder");
            }
            get
            {
                return browseFolder;
            }
        }

        public string Status
        {
            set
            {
                status = value;
                RaisePropertyChanged("Status");
            }
            get
            {
                return status;
            }
        }

        public ICommand Download
        {
            set
            {
                download = value;
                RaisePropertyChanged("Download");
            }
            get
            {
                return download;
            }
        }

        public ICommand Refresh
        {
            set
            {
                refresh = value;
                RaisePropertyChanged("Refresh");
            }
            get
            {
                return refresh;
            }
        }

        public List<FileSystemEntity> LastSelectedEntity
        {
            get { return lastSelectedEntity; }
            set
            {
                lastSelectedEntity = value;
                RaisePropertyChanged("LastSelectedEntity");
            }
        }

        public FileSystemEntity CurrentItem
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
