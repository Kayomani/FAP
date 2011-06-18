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
using System.Collections.Specialized;
using System.Linq;
using System.Waf.Applications;
using FAP.Application.ViewModels;
using FAP.Domain.Entities;
using Fap.Foundation;

namespace FAP.Application.Controllers
{
    public class DownloadQueueController
    {
        private readonly Model model;
        private readonly DownloadQueueViewModel vm;

        public DownloadQueueController(DownloadQueueViewModel vm, Model model)
        {
            this.vm = vm;
            this.model = model;
        }

        public DownloadQueueViewModel ViewModel
        {
            get { return vm; }
        }

        public void Initalise()
        {
            vm.DownloadQueue = new SafeObservingCollection<DownloadRequest>(model.DownloadQueue.List);
            vm.RemoveAll = new DelegateCommand(RemoveAll);
            vm.RemoveSelection = new DelegateCommand(RemoveSelection);
            vm.Moveup = new DelegateCommand(Moveup);
            vm.Movedown = new DelegateCommand(Movedown);
            vm.Movetotop = new DelegateCommand(Movetotop);
            vm.Movetobottom = new DelegateCommand(Movetobottom);
            vm.ClearDownloadLog = new DelegateCommand(ClearDownloadLog);
            vm.ClearUploadLog = new DelegateCommand(ClearUploadLog);
            vm.CompletedDownloads = model.UICompletedDownloads;
            vm.CompletedUploads = model.UICompletedUploads;
            model.CompletedDownloads.CollectionChanged += CompletedDownloads_CollectionChanged;
            model.CompletedUploads.CollectionChanged += CompletedUploads_CollectionChanged;
            CompletedUploads_CollectionChanged(null, null);
            CompletedDownloads_CollectionChanged(null, null);
        }

        private void CompletedUploads_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            List<TransferLog> list = model.CompletedUploads.ToList();
            long totalSize = list.Sum(s => s.Size);


            long speed = 0;
            if (0 != list.Count)
                speed = list.Sum(s => (long) s.Speed)/list.Count;
            vm.UploadStats = string.Format("{0} transfered in {1} files at an average of {2}",
                                           Utility.FormatBytes(totalSize), list.Count,
                                           Utility.ConvertNumberToTextSpeed(speed));
            list.Clear();
        }

        private void CompletedDownloads_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            List<TransferLog> list = model.CompletedDownloads.ToList();
            long totalSize = list.Sum(s => s.Size);
            long speed = 0;
            if (list.Count != 0)
                speed = list.Sum(s => (long) s.Speed)/list.Count;

            vm.DownloadStats = string.Format("{0} transfered in {1} files at an average of {2}",
                                             Utility.FormatBytes(totalSize), list.Count,
                                             Utility.ConvertNumberToTextSpeed(speed));
            list.Clear();
        }

        private void ClearUploadLog()
        {
            model.CompletedUploads.Clear();
        }

        private void ClearDownloadLog()
        {
            model.CompletedDownloads.Clear();
        }

        private List<DownloadRequest> ConvertList(object o)
        {
            var incoming = o as ObservableCollection<object>;
            var list = new List<DownloadRequest>();
            if (null != incoming)
            {
                foreach (DownloadRequest item in incoming)
                {
                    list.Add(item);
                }
            }
            return list;
        }

        private void Movetobottom(object o)
        {
            List<DownloadRequest> items = ConvertList(o);
            try
            {
                model.DownloadQueue.List.Lock();

                for (int i = items.Count - 1; i >= 0; i--)
                {
                    int index = model.DownloadQueue.List.IndexOf(items[i]);
                    if (index >= 0)
                    {
                        model.DownloadQueue.List.RemoveAt(index);
                        model.DownloadQueue.List.Add(items[i]);
                    }
                }
            }
            finally
            {
                model.DownloadQueue.List.Unlock();
            }
        }

        private void Movetotop(object o)
        {
            List<DownloadRequest> items = ConvertList(o);
            try
            {
                model.DownloadQueue.List.Lock();

                for (int i = items.Count - 1; i >= 0; i--)
                {
                    int index = model.DownloadQueue.List.IndexOf(items[i]);

                    if (index > 0)
                    {
                        model.DownloadQueue.List.RemoveAt(index);
                        model.DownloadQueue.List.Insert(0, items[i]);
                    }
                }
            }
            finally
            {
                model.DownloadQueue.List.Unlock();
            }
        }

        private void Moveup(object o)
        {
            List<DownloadRequest> items = ConvertList(o);
            try
            {
                model.DownloadQueue.List.Lock();

                for (int i = items.Count - 1; i >= 0; i--)
                {
                    int index = model.DownloadQueue.List.IndexOf(items[i]);

                    if (index > 0)
                    {
                        model.DownloadQueue.List.RemoveAt(index);
                        model.DownloadQueue.List.Insert(index - 1, items[i]);
                    }
                }
            }
            finally
            {
                model.DownloadQueue.List.Unlock();
            }
        }

        private void Movedown(object o)
        {
            List<DownloadRequest> items = ConvertList(o);
            try
            {
                model.DownloadQueue.List.Lock();

                for (int i = items.Count - 1; i >= 0; i--)
                {
                    int index = model.DownloadQueue.List.IndexOf(items[i]);

                    if (index != -1 && index + 1 < model.DownloadQueue.List.Count)
                    {
                        model.DownloadQueue.List.RemoveAt(index);
                        model.DownloadQueue.List.Insert(index + 1, items[i]);
                    }
                }
            }
            finally
            {
                model.DownloadQueue.List.Unlock();
            }
        }

        private void RemoveSelection()
        {
            var items = new List<DownloadQueue>();
            foreach (DownloadQueue item in vm.SelectedItems)
                items.Add(item);
            foreach (DownloadQueue item in items)
                vm.DownloadQueue.Remove(item);
            items.Clear();
        }

        private void RemoveAll()
        {
            model.DownloadQueue.List.Clear();
        }
    }
}