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
using Fap.Application.Views;
using System.IO;
using Fap.Application.ViewModels;
using System.Windows.Forms;
using Fap.Network.Entity;

namespace Fap.Presentation
{
    public class TrayIcon : ITrayIconView
    {
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenu contextMenu;

        private TrayIconViewModel model;

        public TrayIcon()
        {
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            contextMenu = new System.Windows.Forms.ContextMenu();
            notifyIcon.ContextMenu = contextMenu;
            Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Fap.Presentation;component/Images/folder-yellow.ico")).Stream;
            notifyIcon.Icon = new System.Drawing.Icon(iconStream);
            contextMenu.Popup += new EventHandler(contextMenu_Popup);
        }

        private void contextMenu_Popup(object sender, EventArgs e)
        {
            contextMenu.MenuItems.Clear();

            //Add networks
            foreach (var net in model.Model.Networks)
            {
                MenuItem m = new MenuItem();
                m.Text = net.Name;
                m.MenuItems.Add("No peers");
                m.MenuItems[0].Enabled = false;
                m.Popup += new EventHandler(network_popup);
                m.Tag = net;
                contextMenu.MenuItems.Add(m);
            }
            if (model.Model.Networks.Count == 0)
            {
                contextMenu.MenuItems.Add("Not connected");
                contextMenu.MenuItems[0].Enabled = false;
            }
            contextMenu.MenuItems.Add("-");
            //Add static items
            contextMenu.MenuItems.Add("Queue", delegate { model.Queue.Execute(null); });
            contextMenu.MenuItems.Add("Shares", delegate { model.Shares.Execute(null); });
            contextMenu.MenuItems.Add("Settings", delegate { model.Settings.Execute(null); });
            contextMenu.MenuItems.Add("-");
            contextMenu.MenuItems.Add("Open", delegate { model.Open.Execute(null); });
            contextMenu.MenuItems.Add("Exit", delegate { model.Exit.Execute(null); });
        }

        private void network_popup(object sender, EventArgs e)
        {
            MenuItem src = sender as MenuItem;
            if (null != src)
            {
                Domain.Entity.Network network = src.Tag as Domain.Entity.Network;
                if (null != network)
                {
                    var peers = model.Model.Peers.ToList().OrderBy(p=>p.Nickname).ToList();
                    if (peers.Count > 0)
                    {
                        src.MenuItems.Clear();
                        foreach (var peer in peers)
                        {
                            MenuItem m = new MenuItem();
                            m.Text = peer.Nickname;
                            m.Tag = peer;
                            m.Popup += new EventHandler(peer_Popup);

                            MenuItem p = new MenuItem();
                            p.Text = "FAP Shares";
                            p.Tag = peer;
                            m.MenuItems.Add(p);
                            p.Click += delegate
                            {
                                model.ViewShare.Execute(p.Tag as Node);
                            };

                            src.MenuItems.Add(m);
                        }
                    }
                }
            }
        }

        private void peer_Popup(object sender, EventArgs e)
        {
            MenuItem src = sender as MenuItem;
            if (null != src)
            {
                Network.Entity.Node node = src.Tag as Network.Entity.Node;
                if (null != node)
                {
                   
                }
            }
        }

        public bool ShowIcon
        {
            get
            {
                return notifyIcon.Visible;
            }
            set
            {
                notifyIcon.Visible = value;
            }
        }


        public object DataContext
        {
            get
            {
                return model;
            }
            set
            {
                model = value as TrayIconViewModel;
            }
        }
    }
}
