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
using Fap.Domain.Entity;
using Fap.Application.ViewModels;
using Fap.Foundation;
using System.Waf.Applications;
using Fap.Domain;
using System.Windows.Controls;
using Fap.Domain.Services;
using Fap.Network.Entity;
using Fap.Network.Services;
using Fap.Network;
using Fap.Domain.Verbs;
using System.Threading;

namespace Fap.Application.Controllers
{
    public class BrowserController : AsyncControllerBase
    {
        private readonly Node client;
        private BrowserViewModel bvm;
        private Model model;
        private readonly BufferService bufferService;
        private readonly ConnectionService connectionService;
        private readonly ShareInfoService shareInfo;


        public BrowserViewModel ViewModel { get { return bvm; } }

        public BrowserController(BrowserViewModel bvm, Model model, BufferService bufferService, ConnectionService connectionService, Node client, ShareInfoService i)
        {
            this.client = client;
            this.model = model;
            this.bufferService = bufferService;
            this.connectionService = connectionService;
            this.bvm = bvm;
            shareInfo = i;
            bvm.NoCache = model.AlwaysNoCacheBrowsing;
        }

        public void Initalise()
        {
            bvm.Download = new DelegateCommand(Download);
            bvm.Refresh = new DelegateCommand(Refresh);
            bvm.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(bvm_PropertyChanged);
            //Pull down the inital listing
            bvm.Status = "Getting initial share list..";
            Populate("");
        }

        void bvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentPath")
            {
                Populate(bvm.CurrentPath);
            }
        }

        private void Populate(string ent)
        {
            bvm.IsBusy = true;
            if (string.IsNullOrEmpty(ent))
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(PopulateAsync), null);
                return;
            }
            string[] items = ent.Split('\\');
            FileSystemEntity parent = bvm.Root.Where(n => n.Name == items[0]).FirstOrDefault();

            if (string.IsNullOrEmpty(ent))
            {
                //Just the root
                bvm.CurrentItem = parent;
            }
            else
            {
                for (int i = 1; i < items.Length; i++)
                {
                    var search = parent.Items.Where(n => n.Name == items[i]).FirstOrDefault();
                    if (null == search)
                    {
                        FileSystemEntity fse = new FileSystemEntity();
                        fse.FullPath = ent;
                        parent.Items.Add(fse);
                        parent = fse;
                    }
                    else
                    {
                        parent = search;
                    }
                }
            }

            if (!parent.IsPopulated || bvm.NoCache)
            {
                parent.ClearItems();

                ThreadPool.QueueUserWorkItem(new WaitCallback(PopulateAsync), parent);
            }
            else
            {
                bvm.CurrentItem = parent;
                bvm.IsBusy = false;
            }
        }


        private void PopulateAsync(object o)
        {
            FileSystemEntity fse = o as FileSystemEntity;
            if (null != fse)
            {
                //fse.ClearItems();
                Client c = new Client(bufferService, connectionService);
                BrowseVerb cmd = new BrowseVerb(model, shareInfo);
                cmd.Path = fse.FullPath;
                cmd.NoCache = bvm.NoCache;
                if (c.Execute(cmd, client))
                {

                    SafeObservableStatic.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                      new Action(
                       delegate()
                       {
                           bvm.Status = "Download complete (" + cmd.Results.Count + ").";
                           fse.IsPopulated = true;
                           fse.ClearItems();
                           foreach (var result in cmd.Results)
                               fse.AddItem(result);
                           bvm.CurrentItem = fse;
                           bvm.IsBusy = false;
                       }
                      ));
                }

            }
            else
            {
                Client c = new Client(bufferService, connectionService);
                BrowseVerb cmd = new BrowseVerb(model, shareInfo);
                cmd.NoCache = bvm.NoCache;

                if (c.Execute(cmd, client))
                {

                    SafeObservableStatic.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                      new Action(
                       delegate()
                       {
                           bvm.Status = "Download complete (" + cmd.Results.Count + ").";
                           FileSystemEntity ent = new FileSystemEntity();
                           foreach (var result in cmd.Results)
                           {
                               bvm.Root.Add(result);
                               ent.AddItem(result);
                           }
                           ent.IsPopulated = true;
                           bvm.CurrentItem = ent;
                           bvm.IsBusy = false;
                       }
                      ));
                }
            }
        }

        private void Download()
        {
            for (int i = 0; i < bvm.LastSelectedEntity.Count; i++)
            {
                FileSystemEntity ent = bvm.LastSelectedEntity[i];
                model.DownloadQueue.List.Add(new DownloadRequest()
                                                  { Added = DateTime.Now,
                                                    FullPath = ent.FullPath, 
                                                    IsFolder = ent.IsFolder, 
                                                    Size  = ent.Size,
                                                    State = DownloadRequestState.None,
                                                    ClientID =  client.ID,
                                                    Nickname = client.Nickname});

                if (bvm.LastSelectedEntity.Count == 1)
                    bvm.Status = "Queued download of: " + ent.FullPath;
                else
                    bvm.Status = "Queued " + bvm.LastSelectedEntity.Count + " downloads.";
            }
        }


        private void Refresh()
        {
            if (null != bvm.LastSelectedEntity)
            {
                bvm.Status = "Refreshing current file list.. ";
                QueueWork(new DelegateCommand(item_selected_async), bvm.LastSelectedEntity);
            }
        }

        

        void item_Selected(object sender, System.Windows.RoutedEventArgs e)
        {
            TreeViewItem src = e.Source as TreeViewItem;
            
            if (null != src)
            {
               
                if (!src.IsExpanded)
                {
                    src.IsExpanded = true;
                }
                else
                {
                    FileSystemEntity path = src.Tag as FileSystemEntity;
                    if(null!=path)
                      bvm.Status = "Downloading: " + path.FullPath;
                    QueueWork(new DelegateCommand(item_selected_async),path );
                }
                e.Handled = true;
            }
        }

        private void item_selected_async(object input)
        {
            Client c = new Client(bufferService, connectionService);
            BrowseVerb cmd = new BrowseVerb(model, shareInfo);
            cmd.NoCache = bvm.NoCache;
            FileSystemEntity ent = input as FileSystemEntity;
            if (null != ent)
                cmd.Path = ent.FullPath;

          /*  bvm.CurrentDirectory.Clear();
            if (c.Execute(cmd, client))
            {
                SafeObservableStatic.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                  new Action(
                   delegate()
                   {
                       bvm.Status = "Download complete (" + cmd.Results.Count + ").";
                       foreach (var result in cmd.Results)
                       {
                           bvm.CurrentDirectory.Add(result);
                       }
                   }
                  ));
            }*/
        }

        private void item_Expanded(object sender, System.Windows.RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            item.Items.Clear();
            FileSystemEntity path = item.Tag as FileSystemEntity;
            if(null!=path)
              bvm.Status = "Downloading: " + path.FullPath;
            QueueWork(new DelegateCommand(item_Expanded_Async), new ExpandRequest() { Item = item, Path = path });
            e.Handled = true;
        }

        private void item_Expanded_Async(object input)
        {
            ExpandRequest req = input as ExpandRequest;
            Client c = new Client(bufferService, connectionService);
            BrowseVerb cmd = new BrowseVerb(model, shareInfo);
            cmd.Path = req.Path.FullPath;
            cmd.NoCache = bvm.NoCache;
            if (c.Execute(cmd, client))
            {
                /*  SafeObservableStatic.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                 new Action(
                  delegate()
                  {
                      bvm.Status = "Download complete (" + cmd.Results.Count + " items).";
                    bvm.CurrentDirectory.Clear();
                      foreach (var result in cmd.Results)
                      {
                          if (result.IsFolder)
                          {
                              TreeViewItem x = new TreeViewItem();
                              x.Items.Add(_dummyNode);
                              x.Expanded += new System.Windows.RoutedEventHandler(item_Expanded);
                              x.Selected += new System.Windows.RoutedEventHandler(item_Selected);
                              x.Header = result.Name;
                              x.Tag = result;
                              req.Item.Items.Add(x);
                          }
                          bvm.CurrentDirectory.Add(result);
                      }
                  }
                 ));*/
            }
        }

        private class ExpandRequest
        {
            public FileSystemEntity Path { set; get; }
            public TreeViewItem Item { set; get; }
        }
    }
}
