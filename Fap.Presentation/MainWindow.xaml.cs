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
using Fap.Network.Entity;
using Fap.Domain.Entity;

namespace Fap.Presentation
{
	/// <summary>
	/// The main window
	/// </summary>
    public partial class MainWindow : Window, IMainWindow
	{
		public MainWindow()
		{
			this.InitializeComponent();
			
			// Insert code required on object creation below this point.
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(MainWindow2_DataContextChanged);
            this.Closing += new System.ComponentModel.CancelEventHandler(MainWindow2_Closing);
            
		}

        void MainWindow2_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var c = (DataContext as MainWindowViewModel);
            if (null!=c &&!c.AllowClose)
            {
                e.Cancel = true;
                c.Visible = false;
            }
        }

        void MainWindow2_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
             var c = (DataContext as MainWindowViewModel);
             if (null != c)
             {
                 c.ChatMessages.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ChatMessages_CollectionChanged);
                 c.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(c_PropertyChanged);
             }
        }

        void c_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (null != Model)
             {
                 switch (e.PropertyName)
                 {
                     case "Visible":
                         if (Model.Visible)
                             Show();
                         else
                             Hide();
                         break;
                 }
             }
        }


        private MainWindowViewModel Model
        {
            get
            {
                var c = (DataContext as MainWindowViewModel);
                return c;
            }
        }

        public void Flash()
        {
            FlashWindow.Flash(this, 3);
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
            if (null != Model)
            {
                Model.ViewShare.Execute(Model.SelectedClient);
            }
        }

        private void chatBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

        }

        private void inputText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.IsDown)
            {
                if (null != Model)
                {
                    Model.CurrentChatMessage = chatTextBox.Text;
                    Model.SendChatMessage.Execute(Model.CurrentChatMessage);
                }
            }
        }

        private void DockPanel_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            System.Windows.Controls.ListBox src = e.Source as System.Windows.Controls.ListBox;
            if (null != src)
            {
                MenuItem m = new MenuItem();
                src.ContextMenu.Items.Clear();
                if (src.SelectedItems.Count > 0)
                {
                    Node peer = src.SelectedItem as Node;
                    if (null != peer && peer.NodeType !=ClientType.Overlord)
                    {
                        //View shares
                        m.Foreground = Brushes.Black;
                        m.Header = "View Shares";
                        m.CommandParameter = peer;
                        m.Command = Model.ViewShare;
                        src.ContextMenu.Items.Add(m);
                        //Send message
                        m = new MenuItem();
                        m.Foreground = Brushes.Black;
                        m.Header = "Send Message";
                        m.CommandParameter = peer;
                        m.Command = Model.Chat;
                        src.ContextMenu.Items.Add(m);

                        src.ContextMenu.Items.Add(new Separator());
                        //Check to see if we have received service info
                        if (peer.IsKeySet("HTTP"))
                        {
                            //Received info so display them as appriate
                            string http = peer.GetData("HTTP");
                            string ftp = peer.GetData("FTP");
                            string shares = peer.GetData("Shares");
                            bool added = false;


                            if (!string.IsNullOrEmpty(http))
                            {
                                m = new MenuItem();
                                m.Foreground = Brushes.Black;
                                m.Header = "Web site (" + http + ")";
                                m.CommandParameter = "http://" + peer.Host;
                                m.Command = Model.OpenExternal;
                                src.ContextMenu.Items.Add(m);
                                added = true;
                            }

                            if (!string.IsNullOrEmpty(ftp))
                            {
                                m = new MenuItem();
                                m.Foreground = Brushes.Black;
                                m.Header = "FTP (" + ftp + ")";
                                m.CommandParameter = "ftp://" + peer.Host;
                                m.Command = Model.OpenExternal;
                                src.ContextMenu.Items.Add(m);
                                added = true;
                            }

                            if (!string.IsNullOrEmpty(shares))
                            {
                                int shareCount = shares.Split('|').Length;
                                m = new MenuItem();
                                m.Foreground = Brushes.Black;
                                m.Header = "Network Shares (" + shareCount + ")";
                                m.SubmenuOpened += new RoutedEventHandler(m_SubmenuOpened);
                                m.DataContext = peer;
                                MenuItem sub = new MenuItem();
                                m.Items.Add(sub);
                                src.ContextMenu.Items.Add(m);
                                added = true;
                            }
                            if (added)
                                src.ContextMenu.Items.Add(new Separator());
                        }
                        else
                        {
                            m = new MenuItem();
                            m.Foreground = Brushes.Black;
                            m.Header = "Finding services..";
                            m.IsEnabled = false;
                            src.ContextMenu.Items.Add(m);
                            src.ContextMenu.Items.Add(new Separator());
                        }
                       

                        //View Info
                        /* m = new MenuItem();
                         m.Foreground = Brushes.Black;
                         m.Header = "View Information";
                         m.CommandParameter = peer;
                         m.Command = Model.UserInfo;
                         src.ContextMenu.Items.Add(m);*/
                    }
                }
                //Sort menu
                MenuItem sort = m = new MenuItem();
                m.Foreground = Brushes.Black;
                m.Header = "Sort";
                src.ContextMenu.Items.Add(m);
                //By name
                m = new MenuItem();
                m.Foreground = Brushes.Black;
                m.Header = "By name";
                m.CommandParameter = PeerSortType.Name;
                m.Command = Model.ChangePeerSort;
                if (Model.PeerSortType == PeerSortType.Name)
                    m.FontWeight = FontWeights.DemiBold;
                sort.Items.Add(m);
                //By size
                m = new MenuItem();
                m.Foreground = Brushes.Black;
                m.Header = "By size";
                m.CommandParameter = PeerSortType.Size;
                m.Command = Model.ChangePeerSort;
                if (Model.PeerSortType == PeerSortType.Size)
                    m.FontWeight = FontWeights.DemiBold;
                sort.Items.Add(m);
                //By address
                m = new MenuItem();
                m.Foreground = Brushes.Black;
                m.Header = "By address";
                m.CommandParameter = PeerSortType.Address;
                m.Command = Model.ChangePeerSort;
                if (Model.PeerSortType == PeerSortType.Address)
                    m.FontWeight = FontWeights.DemiBold;
                sort.Items.Add(m);
                //By type
                m = new MenuItem();
                m.Foreground = Brushes.Black;
                m.Header = "By type";
                m.CommandParameter = PeerSortType.Type;
                m.Command = Model.ChangePeerSort;
                if (Model.PeerSortType == PeerSortType.Type)
                    m.FontWeight = FontWeights.DemiBold;
                sort.Items.Add(m);
            }
        }

        void m_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem parent = e.Source as System.Windows.Controls.MenuItem;
            if (null != parent)
            {
                Node peer = parent.DataContext as Node;
                if (null != peer)
                {
                    parent.Items.Clear();
                    string[] shares = peer.GetData("Shares").Split('|');
                    foreach (var share in shares)
                    {
                        MenuItem  m = new MenuItem();
                        m.Foreground = Brushes.Black;
                        m.Header = share;
                        m.CommandParameter = "\\\\" + peer.Host + "\\" + share + "\\";
                        m.Command = Model.OpenExternal;
                        parent.Items.Add(m);
                    }
                }
            }
        }
	}
}