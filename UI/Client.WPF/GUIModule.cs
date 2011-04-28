using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using FAP.Application.Views;
using Fap.Presentation.Panels;
using System.Waf.Presentation.Services;
using System.Waf.Applications.Services;

namespace Fap.Presentation
{
    public class GUIModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MainWindow>().As<IMainWindow>();
            builder.RegisterType<MessageBox>().As<IMessageBoxView>();
            builder.RegisterType<Fap.Presentation.Panels.DownloadQueue>().As<IDownloadQueue>();
            builder.RegisterType<SettingsPanel>().As<ISettingsView>();
            builder.RegisterType<TabWindow>().As<IPopupWindow>();
            builder.RegisterType<Query>().As<IQuery>();
            builder.RegisterType<BrowsePanel>().As<IBrowserView>();
            builder.RegisterType<LogPanel>().As<ILogView>();
            builder.RegisterType<SharesPanel>().As<ISharesView>();
            builder.RegisterType<TrayIcon>().As<ITrayIconView>();
            builder.RegisterType<ComparePanel>().As<ICompareView>();
            builder.RegisterType<Fap.Presentation.Panels.Conversation>().As<IConverstationView>();
            builder.RegisterType<UserInfoPanel>().As<IUserInfo>();
            builder.RegisterType<InterfaceSelection>().As<IInterfaceSelectionView>();
            builder.RegisterType<MessageService>().As<IMessageService>();
        }
    }
}
