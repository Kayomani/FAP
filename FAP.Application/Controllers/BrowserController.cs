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
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Waf.Applications;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using FAP.Application.ViewModels;
using FAP.Domain;
using FAP.Domain.Entities;
using FAP.Domain.Entities.FileSystem;
using FAP.Domain.Net;
using FAP.Domain.Services;
using FAP.Domain.Verbs;
using Fap.Foundation;

namespace FAP.Application.Controllers
{
    public class BrowserController
    {
        private readonly BrowserViewModel bvm;
        private readonly Node client;
        private readonly Model model;
        private readonly ShareInfoService shareInfo;

        public BrowserController(BrowserViewModel bvm, Model model, Node client, ShareInfoService i)
        {
            this.client = client;
            this.model = model;
            this.bvm = bvm;
            shareInfo = i;
            bvm.NoCache = model.AlwaysNoCacheBrowsing;
        }

        public BrowserViewModel ViewModel
        {
            get { return bvm; }
        }

        public void Initalise()
        {
            bvm.Download = new DelegateCommand(Download);
            bvm.Refresh = new DelegateCommand(Refresh);
            bvm.PropertyChanged += bvm_PropertyChanged;
            //Pull down the inital listing
            bvm.Status = "Getting initial share list..";
            Populate("");
        }

        private void bvm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentPath")
            {
                Populate(bvm.CurrentPath);
            }
        }

        private void Populate(string ent)
        {
            ent = ent.Replace('/', '\\');
            bvm.IsBusy = true;
            if (string.IsNullOrEmpty(ent))
            {
                bvm.Root.Clear();
                ThreadPool.QueueUserWorkItem(PopulateAsync, null);
                return;
            }
            string[] items = ent.Split('\\');
            BrowsingFile parent = bvm.Root.Where(n => n.Name == items[0]).FirstOrDefault();

            if (string.IsNullOrEmpty(ent))
            {
                //Just the root
                bvm.CurrentItem = parent;
            }
            else
            {
                for (int i = 1; i < items.Length; i++)
                {
                    BrowsingFile search = parent.Items.Where(n => n.Name == items[i]).FirstOrDefault();
                    if (null == search)
                    {
                        var fse = new BrowsingFile();
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

                ThreadPool.QueueUserWorkItem(PopulateAsync, parent);
            }
            else
            {
                bvm.CurrentItem = parent;
                bvm.IsBusy = false;
            }
        }


        private void PopulateAsync(object o)
        {
            var fse = o as BrowsingFile;
            if (null != fse)
            {
                var c = new Client(model.LocalNode);
                var cmd = new BrowseVerb(model, shareInfo);
                cmd.Path = fse.FullPath;
                cmd.NoCache = bvm.NoCache;
                if (c.Execute(cmd, client))
                {
                    SafeObservableStatic.Dispatcher.Invoke(DispatcherPriority.Normal,
                                                           new Action(
                                                               delegate
                                                                   {
                                                                       bvm.Status = "Download complete (" +
                                                                                    cmd.Results.Count + ").";
                                                                       fse.IsPopulated = true;
                                                                       fse.ClearItems();

                                                                       foreach (BrowsingFile result in cmd.Results)
                                                                       {
                                                                           result.Path = fse.FullPath;
                                                                           fse.AddItem(result);
                                                                       }
                                                                       bvm.CurrentItem = fse;
                                                                       bvm.IsBusy = false;
                                                                   }
                                                               ));
                }
            }
            else
            {
                var c = new Client(model.LocalNode);
                var cmd = new BrowseVerb(model, shareInfo);
                cmd.NoCache = bvm.NoCache;

                if (c.Execute(cmd, client))
                {
                    SafeObservableStatic.Dispatcher.Invoke(DispatcherPriority.Normal,
                                                           new Action(
                                                               delegate
                                                                   {
                                                                       bvm.Status = "Download complete (" +
                                                                                    cmd.Results.Count + ").";
                                                                       var ent = new BrowsingFile();
                                                                       foreach (BrowsingFile result in cmd.Results)
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
                BrowsingFile ent = bvm.LastSelectedEntity[i];

                model.DownloadQueue.List.Add(new DownloadRequest
                                                 {
                                                     Added = DateTime.Now,
                                                     FullPath = ent.FullPath,
                                                     IsFolder = ent.IsFolder,
                                                     Size = ent.Size,
                                                     State = DownloadRequestState.None,
                                                     ClientID = client.ID,
                                                     Nickname = client.Nickname
                                                 });

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
                ThreadPool.QueueUserWorkItem(item_selected_async, bvm.LastSelectedEntity);
            }
        }


        private void item_Selected(object sender, RoutedEventArgs e)
        {
            var src = e.Source as TreeViewItem;

            if (null != src)
            {
                if (!src.IsExpanded)
                {
                    src.IsExpanded = true;
                }
                else
                {
                    var path = src.Tag as BrowsingFile;
                    if (null != path)
                        bvm.Status = "Downloading: " + path.FullPath;
                    ThreadPool.QueueUserWorkItem(item_selected_async, path);
                }
                e.Handled = true;
            }
        }

        private void item_selected_async(object input)
        {
            var c = new Client(model.LocalNode);
            var cmd = new BrowseVerb(model, shareInfo);
            cmd.NoCache = bvm.NoCache;
            var ent = input as BrowsingFile;
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

        private void item_Expanded(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem) sender;
            item.Items.Clear();
            var path = item.Tag as BrowsingFile;
            if (null != path)
                bvm.Status = "Downloading: " + path.FullPath;
            ThreadPool.QueueUserWorkItem(item_Expanded_Async, new ExpandRequest {Item = item, Path = path});
            e.Handled = true;
        }

        private void item_Expanded_Async(object input)
        {
            var req = input as ExpandRequest;
            var c = new Client(model.LocalNode);
            var cmd = new BrowseVerb(model, shareInfo);
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

        #region Nested type: ExpandRequest

        private class ExpandRequest
        {
            public BrowsingFile Path { set; get; }
            public TreeViewItem Item { set; get; }
        }

        #endregion
    }
}