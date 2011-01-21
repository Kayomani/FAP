using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fap.Network.Entity;
using System.Threading;

namespace Fap.Domain.Entity
{
    public class ServerUploadToken
    {
        private int position = 0;
        private bool canUpload = false;
        private Node remoteClient;

        private AutoResetEvent sync = new AutoResetEvent(true);

        public int Position
        {
            set
            {
                lock (sync)
                    position = value;
                sync.Set();
            }
            get
            {
                lock (sync)
                    return position;
            }
        }

        public bool CanUpload
        {
            set
            {
                lock (sync)
                    canUpload = value;
                sync.Set();
            }
            get
            {
                lock (sync)
                    return canUpload;
            }
        }

        public Node RemoteClient
        {
            set
            {
                lock (sync)
                    remoteClient = value;
            }
            get
            {
                lock (sync)
                    return remoteClient;
            }
        }

        public void Wait()
        {
            sync.WaitOne();
        }
    }
}
