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
using Fap.Foundation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Media.Animation;

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
        private BrowserViewModel Model
        {
            get { return DataContext as BrowserViewModel; }
        }

        private void BrowsePanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (Model != null)
             {
                 bar.PathChanged += new RoutedPropertyChangedEventHandler<string>(bar_PathChanged);
                 Model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Model_PropertyChanged);

                 root.SubItems = Model.Root;
                 root.IsPopulated = true;
                 rootB.DataContext = root;
                 rootB.Header = "ROOT";
                 rootB.Items.Add(fake);
                 bar.AddChild(rootB);
                 if (Model.Root.Count == 0)
                     Model.IsBusy = true;
             }
        }

        BreadcrumbItem rootB = new BreadcrumbItem();


        private void bar_PathChanged(object sender, RoutedPropertyChangedEventArgs<string> e)
        {
            if (bar.Path != Model.CurrentPath)
            {
                Model.CurrentPath = bar.Path;
            }
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
            if (ignoreFolderTreeEvents)
                return;
            FileSystemEntity ent = e.NewValue as FileSystemEntity;
            Model.CurrentPath = ent.FullPath;
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            if (ignoreFolderTreeEvents)
                return;
            System.Windows.Controls.TreeViewItem src = e.OriginalSource as System.Windows.Controls.TreeViewItem;
            
            FileSystemEntity ent = src.DataContext as FileSystemEntity;
            ent.ClearItems();
            Model.CurrentPath = ent.FullPath;
        }


        private void listView2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Fap.Domain.Entity.FileSystemEntity item = listView2.SelectedItem as Fap.Domain.Entity.FileSystemEntity;

            if (null != item && null != Model)
            {
                if (item.IsFolder)
                {
                    //Open the sub folder
                    Model.CurrentPath = item.FullPath;
                }
                else
                {

                    Model.LastSelectedEntity = new List<FileSystemEntity>();
                    Model.LastSelectedEntity.Add(item);
                    Model.Download.Execute(null);
                }
            }
        }

        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentPath")
            {
                if (Model.CurrentItem != null && Model.CurrentPath != bar.Path)
                {
                    bar.PathChanged -= new RoutedPropertyChangedEventHandler<string>(bar_PathChanged);
                    rootB.Items.Clear();
                    bar.Path = Model.CurrentPath;
                    bar.PathChanged += new RoutedPropertyChangedEventHandler<string>(bar_PathChanged);

                }

                ignoreFolderTreeEvents = true;
                var entity = GetEntityFromPath(Model.CurrentPath);

                var container = foldersTree.ContainerFromItem(entity);
                if (container != null)
                {
                    container.IsSelected = true;
                    container.IsExpanded = true;
                    container.BringIntoView();
                }
                ignoreFolderTreeEvents = false;
            }
            else if (e.PropertyName == "IsBusy")
            {
                if (Model.IsBusy)
                {
                    da = new DoubleAnimation(100, new Duration(new TimeSpan(0, 0, 8)));
                    da.FillBehavior = FillBehavior.HoldEnd;
                    bar.BeginAnimation(BreadcrumbBar.ProgressValueProperty, da);
                }
                else
                {
                    bar.BeginAnimation(BreadcrumbBar.ProgressValueProperty, null);
                    da = null;
                }
            }
        }


        DoubleAnimation da;

        private bool ignoreFolderTreeEvents = false;

        private FileSystemEntity GetEntityFromPath(string path)
        {
            string[] items = path.Split('\\');
            FileSystemEntity parent = Model.Root.Where(n => n.Name == items[0]).FirstOrDefault();

            if (string.IsNullOrEmpty(path))
            {
                //Just the root
                return null;
            }
            else
            {
                for (int i = 1; i < items.Length; i++)
                {
                    var search = parent.Items.Where(n => n.Name == items[i]).FirstOrDefault();
                    if (null == search)
                    {
                        return null;
                    }
                    else
                    {
                        parent = search;
                    }
                }
            }

            return parent;
        }


        private FileSystemEntity fake = new FileSystemEntity();
        private FileSystemEntity root = new FileSystemEntity();

        /// <summary>
        /// A BreadcrumbItem needs to populate it's Items. This can be due to the fact that a new BreadcrumbItem is selected, and thus
        /// it's Items must be populated to determine whether this BreadcrumbItem show a dropdown button,
        /// or when the Path property of the BreadcrumbBar is changed and therefore the Breadcrumbs must be populated from the new path.
        /// </summary>
        private void BreadcrumbBar_PopulateItems(object sender, Odyssey.Controls.BreadcrumbItemEventArgs e)
        {
            BreadcrumbItem item = e.Item;
            FileSystemEntity fse = item.Data as FileSystemEntity;


            if (item.Data == root)
            {
                if (item.Items.Count == 0)
                {
                    foreach (var i in Model.Root)
                        item.Items.Add(i);
                }
                return;
            }
            if (item.Items.Contains(fake) || item.Items.Count==0)
            {
                if (fse == root)
                {
                    if (fse.IsPopulated)
                    {
                        item.Items.Clear();
                        foreach (var i in root.Folders)
                            item.Items.Add(i);
                    }
                    else
                    {
                        //Add a fake one so we get the selection image
                        item.Items.Add(fake);
                    }
                }
                else
                {
                    if (fse.IsPopulated)
                    {
                        item.Items.Clear();
                        foreach (var i in fse.Folders)
                            item.Items.Add(i);
                    }
                    else
                    {
                        //Add a fake one so we get the selection image
                        item.Items.Add(fake);
                    }
                }
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
           BreadcrumbItem item = e.Item;

            FileSystemEntity ent = item.Data as FileSystemEntity;
            if (null != ent)
            {
                item.Items.Clear();
                if (ent.IsPopulated)
                {
                    foreach (var i in ent.Items)
                    {
                        if (i.IsFolder)
                            item.Items.Add(i);
                    }
                }
            }
            /* 
            // only repopulate, if the BreadcrumbItem is dynamically generated which means, item.Data is a  pointer to itself:
            if (!(item.Data is BreadcrumbItem))
            {
                item.Items.Clear();
                PopulateFolders(item);
            }*/
        }

    }


    public static class TreeViewExtensions
    {

        public static TreeViewItem ContainerFromItem(this TreeView treeView, object item)
        {

            TreeViewItem containerThatMightContainItem = (TreeViewItem)treeView.ItemContainerGenerator.ContainerFromItem(item);

            if (containerThatMightContainItem != null)

                return containerThatMightContainItem;

            else

                return ContainerFromItem(treeView.ItemContainerGenerator, treeView.Items, item);

        }



        private static TreeViewItem ContainerFromItem(ItemContainerGenerator parentItemContainerGenerator, ItemCollection itemCollection, object item)
        {

            foreach (object curChildItem in itemCollection)
            {

                TreeViewItem parentContainer = (TreeViewItem)parentItemContainerGenerator.ContainerFromItem(curChildItem);

                if (parentContainer == null)
                    return null;

                TreeViewItem containerThatMightContainItem = (TreeViewItem)parentContainer.ItemContainerGenerator.ContainerFromItem(item);

                if (containerThatMightContainItem != null)

                    return containerThatMightContainItem;

                TreeViewItem recursionResult = ContainerFromItem(parentContainer.ItemContainerGenerator, parentContainer.Items, item);

                if (recursionResult != null)

                    return recursionResult;

            }

            return null;

        }



        public static object ItemFromContainer(this TreeView treeView, TreeViewItem container)
        {

            TreeViewItem itemThatMightBelongToContainer = (TreeViewItem)treeView.ItemContainerGenerator.ItemFromContainer(container);

            if (itemThatMightBelongToContainer != null)

                return itemThatMightBelongToContainer;

            else

                return ItemFromContainer(treeView.ItemContainerGenerator, treeView.Items, container);

        }



        private static object ItemFromContainer(ItemContainerGenerator parentItemContainerGenerator, ItemCollection itemCollection, TreeViewItem container)
        {

            foreach (object curChildItem in itemCollection)
            {

                TreeViewItem parentContainer = (TreeViewItem)parentItemContainerGenerator.ContainerFromItem(curChildItem);

                TreeViewItem itemThatMightBelongToContainer = (TreeViewItem)parentContainer.ItemContainerGenerator.ItemFromContainer(container);

                if (itemThatMightBelongToContainer != null)

                    return itemThatMightBelongToContainer;

                TreeViewItem recursionResult = ItemFromContainer(parentContainer.ItemContainerGenerator, parentContainer.Items, container) as TreeViewItem;

                if (recursionResult != null)

                    return recursionResult;

            }

            return null;

        }

    }
}
