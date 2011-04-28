using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FAP.Domain.Entities
{
    public class TransferSession : BaseEntity
    {
        private bool isDownload;
        private string user;
        private string status;
        private int percent;
        private long speed;
        private long size;
        private ITransferWorker worker;

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
    }
}
