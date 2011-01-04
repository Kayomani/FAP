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
using System.Waf.Applications;
using Fap.Application.Views;
using System.Windows.Input;
using Fap.Foundation;
using Fap.Domain.Entity;

namespace Fap.Application.ViewModels
{
    public class SharesViewModel : ViewModel<ISharesView>
    {
        private ICommand addCommand;
        private ICommand renameCommand;
        private ICommand removeCommand;
        private ICommand refreshCommand;
        private SafeObservable<Share> shares;
        private Share selectedShare;

        public SharesViewModel(ISharesView view)
            : base(view)
        {
        }

        public SafeObservable<Share> Shares
        {
            get { return shares; }
            set
            {
                shares = value;
                RaisePropertyChanged("Shares");
                lock (shares)
                {
                    foreach (var s in shares.ToList())
                    {
                        s.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(s_PropertyChanged);
                    }
                }
                value.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(value_CollectionChanged);
            }
        }

        public Share SelectedShare
        {
            get { return selectedShare; }
            set
            {
                selectedShare = value;
                RaisePropertyChanged("SelectedShare");
            }
        }

        void value_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var n in e.NewItems)
                    {
                        ((Share)n).PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(s_PropertyChanged);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (var n in e.OldItems)
                    {
                        ((Share)n).PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(s_PropertyChanged);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    foreach (var n in e.OldItems)
                    {
                        ((Share)n).PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(s_PropertyChanged);
                    }
                    foreach (var n in e.NewItems)
                    {
                        ((Share)n).PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(s_PropertyChanged);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    if (null != e.OldItems)
                    {
                        foreach (var n in e.OldItems)
                        {
                            ((Share)n).PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(s_PropertyChanged);
                        }
                    }
                    break;
            }
            RaisePropertyChanged("TotalShareSizeString");
        }

        void s_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("TotalShareSizeString");
        }


        public string TotalShareSizeString
        {
            get
            {
                long total = 0;
                lock (Shares)
                {
                    foreach (var share in Shares.ToList())
                    {
                        total += share.Size;
                    }
                }
                return Utility.FormatBytes(total);
            }
        }

        public ICommand AddCommand
        {
            get { return addCommand; }
            set
            {
                addCommand = value;
                RaisePropertyChanged("AddCommand");
            }
        }

        public ICommand RenameCommand
        {
            get { return renameCommand; }
            set
            {
                renameCommand = value;
                RaisePropertyChanged("RenameCommand");
            }
        }
        public ICommand RemoveCommand
        {
            get { return removeCommand; }
            set
            {
                removeCommand = value;
                RaisePropertyChanged("RemoveCommand");
            }
        }
        public ICommand RefreshCommand
        {
            get { return refreshCommand; }
            set
            {
                refreshCommand = value;
                RaisePropertyChanged("RefreshCommand");
            }
        }

    }
}
