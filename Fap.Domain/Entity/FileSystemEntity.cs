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
using Fap.Foundation;
using Fap.Network.Entity;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;

namespace Fap.Domain.Entity
{
    public class FileSystemEntity : INotifyPropertyChanged
    {
        private ObservableCollection<FileSystemEntity> subItems = new ObservableCollection<FileSystemEntity>();
        private FileSystemEntity temp;

        public ObservableCollection<FileSystemEntity> Items
        {
            set { subItems = value; }
            get { return subItems; }
        }


        public void AddItem(FileSystemEntity ent)
        {
            subItems.Add(ent);
        }

        public void ClearItems()
        {
            if (subItems.Count > 0)
                subItems.Clear();
        }

        public FilteredObservableCollection<FileSystemEntity> Folders
        {
            get
            {
                if (!IsPopulated && subItems.Count == 0)
                {
                    temp = new FileSystemEntity() { IsFolder = true };
                    subItems.Add(temp);
                }
                FilteredObservableCollection<FileSystemEntity> lcv = new FilteredObservableCollection<FileSystemEntity>(subItems);
                lcv.Filter = i => ((FileSystemEntity)i).IsFolder;
                return lcv;
            }
        }

        private bool populated;

        public bool IsPopulated 
        {
            set 
            { 
                populated = value;
                if (value)
                {
                    if (subItems.Contains(temp))
                        subItems.Remove(temp);
                }
            }
            get { return populated; }
        }
        public bool IsFolder { set; get; }
        public string Name { set; get; }
        public long Size { set; get; }
        public DateTime LastModified { set; get; }
        public string FullPath
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrEmpty(Path))
                {
                    sb.Append(Path);
                    sb.Append("\\");
                }
                sb.Append(Name);
                return sb.ToString();
            }
            set
            {
                if (value.Contains("\\"))
                {
                    int split = value.LastIndexOf("\\");
                    Path = value.Substring(0, split);
                    Name = value.Substring(split+1, value.Length - (split+1));
                }
                else
                {
                    Name = value;
                }
            }
        }

        public string Path { set; get; }

        public string SizeString
        {
            get
            {
                return Utility.FormatBytes(Size);
            }
        }

        public override string ToString()
        {
            return Name;
        }

        private void NotifyPropertyChanged(string name)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
