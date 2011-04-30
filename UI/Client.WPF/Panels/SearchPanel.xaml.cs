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
using FAP.Application.Views;
using FAP.Application.ViewModel;

namespace Fap.Presentation.Panels
{
    /// <summary>
    /// Interaction logic for SearchPanel.xaml
    /// </summary>
    public partial class SearchPanel : UserControl, ISearchView
    {
        public SearchPanel()
        {
            InitializeComponent();
            sizeMultiCombo.SelectedIndex = 0;
            modifiedCombo.SelectedIndex = 0;
            sizeCombo.SelectedIndex = 0;
            this.Loaded += new RoutedEventHandler(SearchPanel_Loaded);
        }

        void SearchPanel_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= new RoutedEventHandler(SearchPanel_Loaded);
            sizeCombo.SelectedIndex = 0;
            sizeMultiCombo.SelectedIndex = 0;
            modifiedCombo.SelectedIndex = 0;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchViewModel model = DataContext as SearchViewModel;
                if (null != model)
                {
                    model.SearchString = searchTbox.Text;
                    model.Search.Execute(null);
                }
            }
        }

        private SearchViewModel Model { get { return DataContext as SearchViewModel; } }

        private void listView2_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (listView2.SelectedItems == null || listView2.SelectedItems.Count == 0)
            {
                e.Handled = true;
                return;
            }

            System.Windows.Controls.ListBox src = e.Source as System.Windows.Controls.ListBox;

            src.ContextMenu.Items.Clear();

            MenuItem m = new MenuItem();
            m.Foreground = Brushes.Black;
            m.Header = "Download selected items";
            m.Command = Model.Download;
            m.CommandParameter = listView2.SelectedItems;
            src.ContextMenu.Items.Add(m);

           /* m = new MenuItem();
            m.Foreground = Brushes.Black;
            m.Header = "View in share";
            m.Command = Model.ViewShare;
            m.CommandParameter = listView2.SelectedItems;
            src.ContextMenu.Items.Add(m);*/
        }

        private void listView2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listView2.SelectedItems != null && listView2.SelectedItems.Count == 1)
            {
                Model.Download.Execute(listView2.SelectedItems);
            }
        }
    }
}
