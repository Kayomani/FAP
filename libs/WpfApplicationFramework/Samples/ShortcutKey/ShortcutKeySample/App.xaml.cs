using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using ShortcutKeySample.Presentation;
using ShortcutKeySample.Applications;

namespace ShortcutKeySample
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ShellWindow shellWindow = new ShellWindow();
            ShellViewModel shellViewModel = new ShellViewModel(shellWindow);
            shellViewModel.Show();
        }
    }
}
