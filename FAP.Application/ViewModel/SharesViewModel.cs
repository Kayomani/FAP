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

using System.Collections.Specialized;
using System.ComponentModel;
using System.Waf.Applications;
using System.Windows.Input;
using FAP.Application.Views;
using FAP.Domain.Entities;
using Fap.Foundation;

namespace FAP.Application.ViewModels
{
    public class SharesViewModel : ViewModel<ISharesView>
    {
        private ICommand addCommand;
        private ICommand refreshCommand;
        private ICommand removeCommand;
        private ICommand renameCommand;
        private Share selectedShare;
        private SafeObservingCollection<Share> shares;

        public SharesViewModel(ISharesView view)
            : base(view)
        {
        }

        public SafeObservingCollection<Share> Shares
        {
            get { return shares; }
            set
            {
                //Already bound so remove binding
                if (null != shares)
                {
                    lock (shares)
                    {
                        foreach (Share s in shares.ToList())
                        {
                            s.PropertyChanged -= s_PropertyChanged;
                        }
                    }
                    value.CollectionChanged -= value_CollectionChanged;
                }

                shares = value;
                //Listen for changes on sub objects
                lock (shares)
                {
                    foreach (Share s in shares.ToList())
                    {
                        s.PropertyChanged += s_PropertyChanged;
                    }
                }
                value.CollectionChanged += value_CollectionChanged;
                RaisePropertyChanged("TotalShareSizeString");
                RaisePropertyChanged("Shares");
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


        public string TotalShareSizeString
        {
            get
            {
                long total = 0;
                lock (Shares)
                {
                    foreach (Share share in Shares.ToList())
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

        private void value_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (object n in e.NewItems)
                    {
                        ((Share) n).PropertyChanged += s_PropertyChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (object n in e.OldItems)
                    {
                        ((Share) n).PropertyChanged -= s_PropertyChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (object n in e.OldItems)
                    {
                        ((Share) n).PropertyChanged -= s_PropertyChanged;
                    }
                    foreach (object n in e.NewItems)
                    {
                        ((Share) n).PropertyChanged += s_PropertyChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (null != e.OldItems)
                    {
                        foreach (object n in e.OldItems)
                        {
                            ((Share) n).PropertyChanged -= s_PropertyChanged;
                        }
                    }
                    break;
            }
            RaisePropertyChanged("TotalShareSizeString");
        }

        private void s_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("TotalShareSizeString");
        }
    }
}