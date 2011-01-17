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
using Fap.Domain.Commands;
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
        private readonly object _dummyNode = null;
        private readonly BufferService bufferService;
        private readonly ConnectionService connectionService;


        public BrowserViewModel ViewModel { get { return bvm; } }

        public BrowserController(BrowserViewModel bvm,  Model model, BufferService bufferService,  ConnectionService connectionService, Node client)
        {
            this.client = client;
            this.model = model;
            this.bufferService = bufferService;
            this.connectionService = connectionService;
            this.bvm = bvm;
        }

        public void Initalise()
        {
            bvm.Download = new DelegateCommand(Download);
            bvm.Refresh = new DelegateCommand(Refresh);
            bvm.BrowseFolder = new DelegateCommand(BrowseFolder);
            bvm.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(bvm_PropertyChanged);
            //Pull down the inital listing
            bvm.Status = "Getting initial share list..";
            bvm.Name = "Kayomani";
            QueueWork(new DelegateCommand(Browse));
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
            string[] items = ent.Split('\\');
            FileSystemEntity parent = bvm.Root;

            if (string.IsNullOrEmpty(ent))
            {
                //Just the root
                bvm.CurrentItem = parent;
            }
            else
            {
                for (int i = 0; i < items.Length; i++)
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

            parent.Items.Clear();
            parent.IsPopulated = false;

            ThreadPool.QueueUserWorkItem(new WaitCallback(PopulateAsync), parent);
        }


        private void PopulateAsync(object o)
        {
            FileSystemEntity fse = o as FileSystemEntity;
            if (null != fse)
            {
                Client c = new Client(bufferService, connectionService);
                BrowseVerb cmd = new BrowseVerb(model);
                    cmd.Path = fse.FullPath;

               //  bvm.CurrentDirectory.Clear();
                  if (c.Execute(cmd, client))
                  {
                     
                      SafeObservableStatic.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(
                         delegate()
                         {
                             bvm.Status = "Download complete (" + cmd.Results.Count + ").";
                             fse.IsPopulated = true;
                             fse.Items.Clear();
                             foreach (var result in cmd.Results)
                                 fse.Items.Add(result);
                             bvm.CurrentItem = fse;
                         }
                        ));
                  }

            }
        }

        private void BrowseFolder(object o )
        {
           /* string[] path = ((string)o).Split('\\');
            System.Windows.Controls.TreeViewItem root = bvm.Folders[0] as System.Windows.Controls.TreeViewItem;

            if (path.Length > 0)
            {
                for (int i = 0; i < bvm.Folders.Count; i++)
                {
                    if (string.Equals(bvm.Folders[i].Header, path[0]))
                    {
                        root = bvm.Folders[i];
                        break;
                    }
                }


                if (null != root)
                {
                    for (int i = 1; i < path.Length ; i++)
                    {
                        for (int x = 0; x < root.Items.Count; x++)
                        {
                            System.Windows.Controls.TreeViewItem subitem = root.Items[x] as System.Windows.Controls.TreeViewItem;
                            string text = (string)subitem.Header;
                            if (string.Equals(path[i], text))
                            {
                                root = root.Items[x] as System.Windows.Controls.TreeViewItem;
                                break;
                            }

                        }

                    }
                    root.IsSelected = true;
                    root.IsExpanded = true;
                }
            }*/
        }

        private void Download()
        {

            for (int i = 0; i < bvm.LastSelectedEntity.Count; i++)
            {
                FileSystemEntity ent = bvm.LastSelectedEntity[i];
                model.DownloadQueue.List.Add(new DownloadRequest() { Added = DateTime.Now, FullPath = ent.FullPath, IsFolder = ent.IsFolder });
                if (bvm.LastSelectedEntity.Count - 1 == i)
                {
                    bvm.Status = "Queued download of: " + ent.FullPath;
                }
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

        private void Browse()
        {
            Populate("");
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
            BrowseVerb cmd = new BrowseVerb(model);
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
            BrowseVerb cmd = new BrowseVerb(model);
            cmd.Path = req.Path.FullPath;
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
