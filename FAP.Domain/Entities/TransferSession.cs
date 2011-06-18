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

namespace FAP.Domain.Entities
{
    public class TransferSession : BaseEntity
    {
        private readonly ITransferWorker worker;
        private bool isDownload;
        private int percent;
        private long size;
        private long speed;
        private string status;
        private string user;

        public TransferSession(ITransferWorker worker)
        {
            this.worker = worker;
        }

        public ITransferWorker Worker
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
            get { return size; }
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
            get { return speed; }
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
            get { return percent; }
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
            get { return status; }
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
            get { return user; }
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
            get { return isDownload; }
        }
    }
}