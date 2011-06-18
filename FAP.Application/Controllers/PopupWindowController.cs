#region Copyright Kayomani 2011.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.

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
using System.ComponentModel;
using System.Linq;
using System.Waf.Applications;
using System.Windows;
using Autofac;
using FAP.Application.ViewModels;
using IContainer = Autofac.IContainer;

namespace FAP.Application.Controllers
{
    public class PopupWindowController
    {
        #region Delegates

        public delegate void TabClosing(object o);

        #endregion

        private readonly IContainer container;

        private PopupWindowViewModel viewModel;
        private bool windowOpen;

        public PopupWindowController(IContainer container)
        {
            this.container = container;
        }

        public object ActiveTab
        {
            get
            {
                var e = viewModel.ActiveDocumentView as PopUpWindowTab;
                if (null != e)
                {
                    var p = e.Content as FrameworkElement;
                    if (null != p)
                        return p.DataContext;
                }
                return null;
            }
        }

        public event TabClosing OnTabClosing;

        public void FlashIfNotActive()
        {
            viewModel.FlashIfNotActive();
        }

        private void setupViewModel()
        {
            if (!windowOpen)
            {
                viewModel = container.Resolve<PopupWindowViewModel>();
                viewModel.Close = new DelegateCommand(windowClosing);
                viewModel.PropertyChanged += viewModel_PropertyChanged;
                windowOpen = true;
            }
        }

        private void viewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveDocumentView")
            {
                PopUpWindowTab search =
                    viewModel.DocumentViews.Where(v => v == viewModel.ActiveDocumentView).FirstOrDefault();
                if (null != search)
                {
                    if (search.Color != "Black")
                        search.Color = "Black";
                }
            }
        }

        public void Close()
        {
            if (null != viewModel)
                viewModel.CloseWindow();
        }

        private void windowClosing(object o)
        {
            foreach (PopUpWindowTab view in viewModel.DocumentViews)
            {
                TabClose(view.ContentViewModel);
            }
            viewModel.DocumentViews.Clear();
            windowOpen = false;
        }

        public void SwitchToTab(object o)
        {
            PopUpWindowTab search = viewModel.DocumentViews.Where(dv => dv.ContentViewModel == o).FirstOrDefault();
            if (null != search)
            {
                viewModel.ActiveDocumentView = search;
            }
        }

        public void Highlight(object o)
        {
            PopUpWindowTab search = viewModel.DocumentViews.Where(v => v.ContentViewModel == o).FirstOrDefault();
            if (null != search)
            {
                search.Color = "red";
            }
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
            viewModel.TabClose = new DelegateCommand(TabClose);
            if (!open)
                viewModel.Show();
        }

        private void TabClose(Object o)
        {
            if (null != OnTabClosing)
                OnTabClosing(o);
        }
    }
}