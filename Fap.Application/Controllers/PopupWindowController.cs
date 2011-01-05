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
using Fap.Application.ViewModels;
using System.Waf.Applications;
using Autofac;

namespace Fap.Application.Controllers
{
    public class PopupWindowController
    {
        private PopupWindowViewModel viewModel;
        private bool windowOpen = false;
        private IContainer container;

        public PopupWindowController(IContainer container)
        {
            this.container = container;
        }


         private void setupViewModel()
         {
             if (!windowOpen)
             {
                 viewModel = container.Resolve<PopupWindowViewModel>();
                 viewModel.Close = new DelegateCommand(windowClosing);
                 windowOpen = true;
             }
         }

         public void Close()
         {
             if (windowOpen)
                 viewModel.CloseWindow();
         }

         private void windowClosing()
         {
             viewModel.DocumentViews.Clear();
             windowOpen = false;
         }

         public void AddWindow(object o, string header)
         {
             bool open = windowOpen;
             setupViewModel();
             var t = new PopUpWindowTab();
             t.Name = header;
             t.Content = o;
             viewModel.DocumentViews.Add(t);
             viewModel.ActiveDocumentView = t;
             if (!open)
                 viewModel.Show();
         }
    }
}
