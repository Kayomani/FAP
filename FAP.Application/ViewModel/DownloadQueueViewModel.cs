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

using System.Collections;
using System.Waf.Applications;
using System.Windows.Input;
using FAP.Application.Views;
using FAP.Domain.Entities;
using Fap.Foundation;

namespace FAP.Application.ViewModels
{
    public class DownloadQueueViewModel : ViewModel<IDownloadQueue>
    {
        private ICommand clearDownloadLog;
        private ICommand clearUploadLog;
        private SafeObservingCollection<TransferLog> completedDownloads;
        private SafeObservingCollection<TransferLog> completedUploads;
        private SafeObservingCollection<DownloadRequest> downloadQueue;
        private string downloadStats;

        private ICommand movedown;
        private ICommand movetobottom;
        private ICommand movetotop;
        private ICommand moveup;
        private ICommand removeAll;
        private ICommand removeSelection;
        private IList selectedItems;
        private string uploadStats;

        public DownloadQueueViewModel(IDownloadQueue view)
            : base(view)
        {
        }

        public string DownloadStats
        {
            get { return downloadStats; }
            set
            {
                downloadStats = value;
                RaisePropertyChanged("DownloadStats");
            }
        }

        public string UploadStats
        {
            get { return uploadStats; }
            set
            {
                uploadStats = value;
                RaisePropertyChanged("UploadStats");
            }
        }

        public ICommand ClearDownloadLog
        {
            get { return clearDownloadLog; }
            set
            {
                clearDownloadLog = value;
                RaisePropertyChanged("ClearDownloadLog");
            }
        }

        public ICommand ClearUploadLog
        {
            get { return clearUploadLog; }
            set
            {
                clearUploadLog = value;
                RaisePropertyChanged("ClearUploadLog");
            }
        }

        public ICommand Movetobottom
        {
            get { return movetobottom; }
            set
            {
                movetobottom = value;
                RaisePropertyChanged("Movetobottom");
            }
        }

        public ICommand Movedown
        {
            get { return movedown; }
            set
            {
                movedown = value;
                RaisePropertyChanged("Movedown");
            }
        }

        public ICommand Moveup
        {
            get { return moveup; }
            set
            {
                moveup = value;
                RaisePropertyChanged("Moveup");
            }
        }

        public ICommand Movetotop
        {
            get { return movetotop; }
            set
            {
                movetotop = value;
                RaisePropertyChanged("Movetotop");
            }
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

        public SafeObservingCollection<DownloadRequest> DownloadQueue
        {
            set
            {
                downloadQueue = value;
                RaisePropertyChanged("DownloadQueue");
            }
            get { return downloadQueue; }
        }

        public SafeObservingCollection<TransferLog> CompletedDownloads
        {
            set
            {
                completedDownloads = value;
                RaisePropertyChanged("CompletedDownloads");
            }
            get { return completedDownloads; }
        }

        public SafeObservingCollection<TransferLog> CompletedUploads
        {
            set
            {
                completedUploads = value;
                RaisePropertyChanged("CompletedUploads");
            }
            get { return completedUploads; }
        }

        public IList SelectedItems
        {
            set
            {
                selectedItems = value;
                RaisePropertyChanged("SelectedItems");
            }
            get { return selectedItems; }
        }
    }
}