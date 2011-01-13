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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Fap.Application.Views;
using Fap.Application.ViewModels;
using Fap.Domain.Entity;
using Odyssey.Controls;

namespace Fap.Presentation.Panels
{
    /// <summary>
    /// Interaction logic for BrowsePanel.xaml
    /// </summary>
    public partial class BrowsePanel : UserControl, IBrowserView
    {
        public BrowsePanel()
        {
            InitializeComponent();
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(BrowsePanel_DataContextChanged);
        }

        private void BrowsePanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
             BrowserViewModel vm = DataContext as BrowserViewModel;
             if (listView2.SelectedItems != null)
             {
                 vm.Folders.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Folders_CollectionChanged);
             }
        }

        private void Folders_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        /*    return;
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
               
                      BreadcrumbItem root = new BreadcrumbItem();
                      foreach (TreeViewItem item in e.NewItems)
                      {
                          FolderItem i = new FolderItem();
                          i.Folder = item.Header.ToString();
                          root.Items.Add(i);
                      }
                      bar.AddChild(root);
                 
                
            }*/
        }

        private void listView2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BrowserViewModel vm = DataContext as BrowserViewModel;
            if (listView2.SelectedItems != null)
            {
                vm.LastSelectedEntity = listView2.SelectedItems.Cast<FileSystemEntity>().ToList(); ;
            }
        }

        private void foldersTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            BrowserViewModel vm = DataContext as BrowserViewModel;
            TreeViewItem i = e.NewValue as TreeViewItem;
            if (null != vm && i !=null)
            {
                FileSystemEntity ent = i.Tag as FileSystemEntity;
                if (null != ent)
                {
                    vm.LastSelectedEntity = new List<FileSystemEntity>();
                    vm.LastSelectedEntity.Add(ent);
                }

            }
        }

        private void listView2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Fap.Domain.Entity.FileSystemEntity item = listView2.SelectedItem as Fap.Domain.Entity.FileSystemEntity;
            BrowserViewModel vm = DataContext as BrowserViewModel;
            if (null != item)
            {
                if (item.IsFolder)
                {
                    //Open the sub folder
                    vm.BrowseFolder.Execute(item.FullPath);
                }
                else
                {

                    vm.LastSelectedEntity = new List<FileSystemEntity>();
                    vm.LastSelectedEntity.Add(item);
                    vm.Download.Execute(null);
                }
            }
        }

        /// <summary>
        /// A BreadcrumbItem needs to populate it's Items. This can be due to the fact that a new BreadcrumbItem is selected, and thus
        /// it's Items must be populated to determine whether this BreadcrumbItem show a dropdown button,
        /// or when the Path property of the BreadcrumbBar is changed and therefore the Breadcrumbs must be populated from the new path.
        /// </summary>
        private void BreadcrumbBar_PopulateItems(object sender, Odyssey.Controls.BreadcrumbItemEventArgs e)
        {
            BreadcrumbItem item = e.Item;
            if (item.Items.Count == 0)
            {
                PopulateFolders(item);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Populate the Items of the specified BreadcrumbItem with the sub folders if necassary.
        /// </summary>
        /// <param name="item"></param>
        private static void PopulateFolders(BreadcrumbItem item)
        {
           /* BreadcrumbBar bar = item.BreadcrumbBar;
            string path = bar.PathFromBreadcrumbItem(item);
            string trace = item.TraceValue;
            if (trace.Equals("Computer"))
            {
                string[] dirs = System.IO.Directory.GetLogicalDrives();
                foreach (string s in dirs)
                {
                    string dir = s;
                    if (s.EndsWith(bar.SeparatorString)) dir = s.Remove(s.Length - bar.SeparatorString.Length, bar.SeparatorString.Length);
                    FolderItem fi = new FolderItem();
                    fi.Folder = dir;

                    item.Items.Add(fi);
                }
            }
            else
            {
                try
                {
                    string[] paths = System.IO.Directory.GetDirectories(path + "\\");
                    foreach (string s in paths)
                    {
                        string file = System.IO.Path.GetFileName(s);
                        FolderItem fi = new FolderItem();
                        fi.Folder = file;
                        item.Items.Add(fi);
                    }
                }
                catch { }
            }*/
        }

        private void BreadcrumbBar_PathConversion(object sender, PathConversionEventArgs e)
        {
           /* if (e.Mode == PathConversionEventArgs.ConversionMode.DisplayToEdit)
            {
                if (e.DisplayPath.StartsWith(@"Computer\", StringComparison.OrdinalIgnoreCase))
                {
                    e.EditPath = e.DisplayPath.Remove(0, 9);
                }
                else if (e.DisplayPath.StartsWith(@"Network\", StringComparison.OrdinalIgnoreCase))
                {
                    string editPath = e.DisplayPath.Remove(0, 8);
                    editPath = @"\\" + editPath.Replace('\\', '/');
                    e.EditPath = editPath;
                }
            }
            else
            {
                if (e.EditPath.StartsWith("c:", StringComparison.OrdinalIgnoreCase))
                {
                    e.DisplayPath = @"Desktop\Computer\" + e.EditPath;
                }
                else if (e.EditPath.StartsWith(@"\\"))
                {
                    e.DisplayPath = @"Desktop\Network\" + e.EditPath.Remove(0, 2).Replace('/', '\\');
                }
            }*/
        }

        /// <summary>
        /// The dropdown menu of a BreadcrumbItem was pressed, so delete the current folders, and repopulate the folders
        /// to ensure actual data.
        /// </summary>
        private void bar_BreadcrumbItemDropDownOpened(object sender, BreadcrumbItemEventArgs e)
        {
           /* BreadcrumbItem item = e.Item;

            // only repopulate, if the BreadcrumbItem is dynamically generated which means, item.Data is a  pointer to itself:
            if (!(item.Data is BreadcrumbItem))
            {
                item.Items.Clear();
                PopulateFolders(item);
            }*/
        }

    }
}
