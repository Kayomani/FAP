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
    }
}
