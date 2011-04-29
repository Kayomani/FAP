using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Application.ViewModels;
using System.Waf.Applications;
using System.Windows;
using Autofac;

namespace FAP.Application.Controllers
{
    public class PopupWindowController
    {
        private PopupWindowViewModel viewModel;
        private bool windowOpen = false;
        private IContainer container;

        public delegate void TabClosing(object o);
        public event TabClosing OnTabClosing;

        public PopupWindowController(IContainer container)
        {
            this.container = container;
        }

        public object ActiveTab
        {
            get
            {
                PopUpWindowTab e = viewModel.ActiveDocumentView as PopUpWindowTab;
                if (null != e)
                {
                    FrameworkElement p = e.Content as FrameworkElement;
                    if (null != p)
                        return p.DataContext;
                }
                return null;
            }
        }

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
                viewModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(viewModel_PropertyChanged);
                windowOpen = true;
            }
        }

        void viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveDocumentView")
            {
                var search = viewModel.DocumentViews.Where(v => v == viewModel.ActiveDocumentView).FirstOrDefault();
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
            foreach (var view in viewModel.DocumentViews)
            {
                TabClose(view.ContentViewModel);
            }
            viewModel.DocumentViews.Clear();
            windowOpen = false;
        }

        public void SwitchToTab(object o)
        {
            var search = viewModel.DocumentViews.Where(dv => dv.ContentViewModel == o).FirstOrDefault();
            if (null != search)
            {
                viewModel.ActiveDocumentView = search;
            }
        }

        public void Highlight(object o)
        {
            var search = viewModel.DocumentViews.Where(v => v.ContentViewModel == o).FirstOrDefault();
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
