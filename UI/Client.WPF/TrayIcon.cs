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
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using FAP.Application.Views;
using FAP.Application.ViewModels;
using FAP.Domain.Entities;
using FAP.Domain;

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
            notifyIcon.DoubleClick += new EventHandler(notifyIcon_DoubleClick);
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu",
                 BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(notifyIcon, null);
            }
            catch { }
        }

        private void contextMenu_Popup(object sender, EventArgs e)
        {
            contextMenu.MenuItems.Clear();

            //Add networks
            /*foreach (var net in model.Model.Networks)
            {
                MenuItem m = new MenuItem();
                m.Text = net.NetworkName;
                m.MenuItems.Add("No peers");
                m.MenuItems[0].Enabled = false;
                m.Popup += new EventHandler(network_popup);
                m.Tag = net;
                contextMenu.MenuItems.Add(m);
            }*/
             MenuItem m = new MenuItem();
             m.Text = model.Model.Network.NetworkName;
                m.MenuItems.Add("No peers");
                m.MenuItems[0].Enabled = false;
                m.Popup += new EventHandler(network_popup);
                m.Tag = model.Model.Network;
                contextMenu.MenuItems.Add(m);
            
            /*if (model.Model.Networks.Count == 0)
            {
                contextMenu.MenuItems.Add("Not connected");
                contextMenu.MenuItems[0].Enabled = false;
            }*/
            contextMenu.MenuItems.Add("-");
            //Add static items
            contextMenu.MenuItems.Add("Compare", delegate { model.Compare.Execute(null); });
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
                Network network = src.Tag as Network;
                if (null != network)
                {
                    var peers = network.Nodes.ToList().OrderBy(p => p.Nickname).ToList();
                    if (peers.Count > 0)
                    {
                        src.MenuItems.Clear();
                        foreach (var peer in peers.Where(p=>p.NodeType!=ClientType.Overlord))
                        {
                            MenuItem main = new MenuItem();
                            main.Text = peer.Nickname;
                            main.Tag = peer;
                            main.Popup += new EventHandler(peer_Popup);

                            MenuItem p = new MenuItem();
                            p.Text = "FAP Shares";
                            p.Tag = peer;
                            main.MenuItems.Add(p);
                            p.Click += delegate
                            {
                                model.ViewShare.Execute(p.Tag as Node);
                            };

                            src.MenuItems.Add(main);
                            MenuItem m = null;
                            if (peer.IsKeySet("HTTP"))
                            {
                                //Received info so display them as appriate
                                string http = peer.GetData("HTTP");
                                string ftp = peer.GetData("FTP");
                                string shares = peer.GetData("Shares");
                                
                                if (!string.IsNullOrEmpty(http))
                                {
                                    m = new MenuItem();
                                    m.Text = "Web site (" + http + ")";
                                    m.Click += delegate
                                    {
                                        model.OpenExternal.Execute("http://" + peer.Host);
                                    };
                                    main.MenuItems.Add(m);
                                }

                                if (!string.IsNullOrEmpty(ftp))
                                {
                                    m = new MenuItem();
                                    m.Text = "FTP (" + ftp + ")";
                                    m.Click += delegate
                                    {
                                        model.OpenExternal.Execute("ftp://" + peer.Host);
                                    };
                                    main.MenuItems.Add(m);
                                }

                                if (!string.IsNullOrEmpty(shares))
                                {
                                    int shareCount = shares.Split('|').Length;
                                    m = new MenuItem();
                                    m.Text = "Network Shares (" + shareCount + ")";

                                    string[] shareslist = peer.GetData("Shares").Split('|');
                                    foreach (var sharename in shareslist)
                                    {
                                        MenuItem sub = new MenuItem();
                                        sub.Text = sharename;
                                        sub.Click += delegate
                                        {
                                            model.OpenExternal.Execute("\\\\" + peer.Host + "\\" + sharename + "\\");
                                        };
                                        m.MenuItems.Add(sub);
                                    }
                                    main.MenuItems.Add(m);
                                }
                            }
                            else
                            {
                                m = new MenuItem();
                                m.Text = "Finding services..";
                                m.Enabled = false;
                                main.MenuItems.Add(m);
                            }

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
                Node node = src.Tag as Node;
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


        public void Dispose()
        {
            notifyIcon.Dispose();
        }
    }
}
