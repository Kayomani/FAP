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
using FAP.Application.ViewModels;
using FAP.Domain.Entities;
using System.Waf.Applications;

namespace FAP.Application.Controllers
{
    public class DownloadQueueController
    {
        private DownloadQueueViewModel vm;
        private Model model;

        public DownloadQueueController(DownloadQueueViewModel vm, Model model)
        {
            this.vm = vm;
            this.model = model;
        }

        public DownloadQueueViewModel ViewModel { get { return vm; } }

        public void Initalise()
        {
            vm.DownloadQueue = new Fap.Foundation.SafeObservingCollection<DownloadRequest>(model.DownloadQueue.List);
            vm.RemoveAll = new DelegateCommand(RemoveAll);
            vm.RemoveSelection = new DelegateCommand(RemoveSelection);
            vm.Moveup = new DelegateCommand(Moveup);
            vm.Movedown = new DelegateCommand(Movedown);
            vm.Movetotop = new DelegateCommand(Movetotop);
            vm.Movetobottom = new DelegateCommand(Movetobottom);
        }

        private List<DownloadRequest> ConvertList(object o)
        {
            System.Collections.ObjectModel.ObservableCollection<object> incoming = o as System.Collections.ObjectModel.ObservableCollection<object>;
            List<DownloadRequest> list = new List<DownloadRequest>();
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
            var items = ConvertList(o);
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
            var items = ConvertList(o);
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
            var items = ConvertList(o);
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
            var items = ConvertList(o);
            try
            {
                model.DownloadQueue.List.Lock();

                for (int i = items.Count - 1; i >= 0; i--)
                {
                    int index = model.DownloadQueue.List.IndexOf(items[i]);

                    if (index!=-1 && index+1<model.DownloadQueue.List.Count)
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
            vm.DownloadQueue.Remove(vm.SelectedItems);
        }

        private void RemoveAll()
        {
            model.DownloadQueue.List.Clear();
        }
    }
}
