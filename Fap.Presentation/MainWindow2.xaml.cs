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
using System.Windows.Shapes;
using System.Windows.Threading;
using Fap.Application.Views;
using Fap.Application.ViewModels;

namespace Fap.Presentation
{
	/// <summary>
	/// Interaction logic for MainWindow2.xaml
	/// </summary>
    public partial class MainWindow2 : Window, IMainWindow
	{
		public MainWindow2()
		{
			this.InitializeComponent();
			
			// Insert code required on object creation below this point.
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(MainWindow2_DataContextChanged);
            this.Closing += new System.ComponentModel.CancelEventHandler(MainWindow2_Closing);
		}

        void MainWindow2_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
             var c = (DataContext as MainWindowViewModel);
             if (!c.AllowClose)
             {
                 e.Cancel = true;
                 if (null != c)
                 {
                     if (c.Closing.CanExecute(null))
                         c.Closing.Execute(null);
                 }
             }
        }

        void MainWindow2_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
             var c = (DataContext as MainWindowViewModel);
             if (null != c)
             {
                 c.ChatMessages.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ChatMessages_CollectionChanged);
             }
        }

        void ChatMessages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        //    if(chatBox.Items.Count>0)
          //    chatBox.ScrollIntoView(chatBox.Items.GetItemAt(chatBox.Items.Count - 1));
          //  ScrollViewer scroll = chatBox.GetScrollViewer();
        }

        public void AddWindow(string title, object content)
        {
            TabItem ti = new TabItem();
            ti.Header = title;
            ContentPresenter cp = new ContentPresenter();
            cp.Content = content;
            ti.Content = cp;
           // tabControl1.Items.Add(ti);

        }


        private void listBox1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var c = (DataContext as MainWindowViewModel);
            if (null != c)
            {

                c.ViewShare.Execute(c.SelectedClient);
            }
        }

        private void chatBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

        }

        private void inputText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.IsDown)
            {
                var c = (DataContext as MainWindowViewModel);
                if (null != c)
                {
                    c.CurrentChatMessage = chatTextBox.Text;
                    c.SendChatMessage.Execute(c.CurrentChatMessage);
                }
            }
        }
	}
}