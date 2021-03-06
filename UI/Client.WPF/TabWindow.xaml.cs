﻿#region Copyright Kayomani 2010.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
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
using System.ComponentModel;
using System.Collections.ObjectModel;
using FAP.Application.Views;
using FAP.Application.ViewModels;

namespace Fap.Presentation
{
    /// <summary>
    /// Interaction logic for TabWindow.xaml
    /// </summary>
    public partial class TabWindow : Window, IPopupWindow
    {

        public TabWindow()
        {
            InitializeComponent();
            this.Closing += new System.ComponentModel.CancelEventHandler(TabWindow_Closing);
            ResizeToScreen();
        }


        public void ResizeToScreen()
        {
            double horSpace = Math.Max(SystemParameters.PrimaryScreenWidth * 0.1, 100);
            double vertSpace = Math.Max(SystemParameters.PrimaryScreenHeight * 0.1, 100);

            Top = vertSpace;
            Left = horSpace;
            Width = SystemParameters.PrimaryScreenWidth - horSpace * 2;
            Height = SystemParameters.PrimaryScreenHeight - vertSpace * 2;
        }

        void TabWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != Model)
            {
                Model.Close.Execute(null);
            }
        }


        private PopupWindowViewModel Model
        {
            get { return DataContext as PopupWindowViewModel; }
        }
      
        private void tabControl_TabItemClosing(object sender, Wpf.Controls.TabItemCancelEventArgs e)
        {
            PopUpWindowTab tab = e.TabItem.DataContext as PopUpWindowTab;
            if (null != tab)
            {
                FrameworkElement p = tab.Content as FrameworkElement;
                if(null!=p)
                {
                    Model.TabClose.Execute(p.DataContext);
                }
            }

            if (tabControl.Items.Count == 1)
                this.Close();
        }

        public void FlashIfNotActive()
        {
            if (!IsActive)
            {
                FlashWindow.Flash(this);
            }
        }
    }

}
