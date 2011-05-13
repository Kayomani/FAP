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
using FAP.Application.Views;
using FAP.Application.ViewModels;
using FAP.Domain;

namespace Fap.Presentation.Panels
{
    /// <summary>
    /// Interaction logic for SettingsPanel.xaml
    /// </summary>
    public partial class SettingsPanel : UserControl, ISettingsView
    {
       

        public SettingsPanel()
        {
            InitializeComponent();
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(SettingsPanel_DataContextChanged);
        }

        private void SettingsPanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SettingsViewModel setting = e.NewValue as SettingsViewModel;
            if (null != setting)
            {
                switch (setting.Model.OverlordPriority)
                {
                    case OverlordPriority.High:
                        overlordpri.SelectedIndex = 0;
                        break;
                    case OverlordPriority.Low:
                        overlordpri.SelectedIndex = 2;
                        break;
                    default:
                        overlordpri.SelectedIndex = 1;
                        break;
                }
            }
        }

        private void overlordpri_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingsViewModel model = DataContext as SettingsViewModel;
            if (null != model)
            {
                switch (overlordpri.SelectedIndex)
                {
                    case 0:
                        model.Model.OverlordPriority = OverlordPriority.High;
                        break;
                    case 2:
                        model.Model.OverlordPriority = OverlordPriority.Low;
                        break;
                    default:
                        model.Model.OverlordPriority = OverlordPriority.Normal;
                        break;
                }
            }
        }
    }
}
