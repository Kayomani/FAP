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
using Fap.Application.Views;

namespace Fap.Presentation
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBox : Window, IMessageBoxView
    {
        private bool ok = true;

        public MessageBox()
        {
            InitializeComponent(); 
        }


        public bool? ShowDialog(object parent)
        {
            Owner = parent as Window;
            ShowDialog();
            return ok;
        }

        public new bool? ShowDialog()
        {
            base.ShowDialog();
            return ok;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ok = true;
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ok = false;
            Close();
        }
    }
}
