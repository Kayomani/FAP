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

using System.Reflection;
using System.Waf.Applications;
using System.Windows.Input;
using FAP.Application.Views;
using FAP.Domain.Entities;
using Fap.Foundation;
using Microsoft.Win32;

namespace FAP.Application.ViewModels
{
    public class SettingsViewModel : ViewModel<ISettingsView>
    {
        private readonly string startupRegistryPath = "SOFTWARE/Microsoft/Windows/CurrentVersion/Run";
        private ICommand changeAvatar;
        private ICommand displayQuickStart;
        private ICommand editDownloadDir;
        private Model model;
        private ICommand resetInterface;

        public SettingsViewModel(ISettingsView view)
            : base(view)
        {
        }

        public ICommand ResetInterface
        {
            get { return resetInterface; }
            set
            {
                resetInterface = value;
                RaisePropertyChanged("ResetInterface");
            }
        }

        public ICommand EditDownloadDir
        {
            get { return editDownloadDir; }
            set
            {
                editDownloadDir = value;
                RaisePropertyChanged("EditDownloadDir");
            }
        }

        public ICommand DisplayQuickStart
        {
            get { return displayQuickStart; }
            set
            {
                displayQuickStart = value;
                RaisePropertyChanged("DisplayQuickStart");
            }
        }

        public ICommand ChangeAvatar
        {
            get { return changeAvatar; }
            set
            {
                changeAvatar = value;
                RaisePropertyChanged("ChangeAvatar");
            }
        }

        public Model Model
        {
            get { return model; }
            set
            {
                model = value;
                RaisePropertyChanged("Model");
            }
        }

        public bool RunOnStartUp
        {
            set
            {
                if (value)
                    RegistryHelper.SetRegistryData(Registry.CurrentUser, startupRegistryPath, "FAP", GetStartupCommand());
                else
                    RegistryHelper.SetRegistryData(Registry.CurrentUser, startupRegistryPath, "FAP", string.Empty);
                RaisePropertyChanged("RunOnStartUp");
            }
            get
            {
                return (RegistryHelper.GetRegistryData(Registry.CurrentUser, startupRegistryPath + "/FAP") ==
                        GetStartupCommand());
            }
        }

        private string GetStartupCommand()
        {
            string location = Assembly.GetEntryAssembly().Location;
            return string.Format("\"{0}\" STARTUP", location);
        }
    }
}