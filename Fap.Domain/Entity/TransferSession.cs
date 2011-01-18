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
using System.ComponentModel;
using Fap.Domain.Services;

namespace Fap.Domain.Entity
{
   public class TransferSession : INotifyPropertyChanged
    {
        private bool isDownload;
        private string user;
        private string status;
        private int percent;
        private long speed;
        private long size;
        private DownloadWorkerService worker;

        public TransferSession(DownloadWorkerService worker)
        {
            this.worker = worker;
        }

        public DownloadWorkerService Worker
        {
            get { return worker; }
        }

        public long Size
        {
            set
            {
                if (value != size)
                {
                    size = value;
                    NotifyChange("Size");
                }
            }
            get
            {
                return size;
            }
        }

        public long Speed
        {
            set
            {
                if (speed != value)
                {
                    speed = value;
                    NotifyChange("Speed");
                }
            }
            get
            {
                return speed;
            }
        }

        public int Percent
        {
            set
            {
                if (value != percent)
                {
                    percent = value;
                    NotifyChange("Percent");
                }
            }
            get
            {
                return percent;
            }
        }

        public string Status
        {
            set
            {
                if (status != value)
                {
                    status = value;
                    NotifyChange("Status");
                }
            }
            get
            {
                return status;
            }
        }

        public string User
        {
            set
            {
                if (value != user)
                {
                    user = value;
                    NotifyChange("User");
                }
            }
            get
            {
                return user;
            }
        }

        public bool IsDownload
        {
            set
            {
                if (value != isDownload)
                {
                    isDownload = value;
                    NotifyChange("IsDownload");
                }
            }
            get
            {
                return isDownload;
            }
        }

        private void NotifyChange(string path)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(path));
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
