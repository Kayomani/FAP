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
using Fap.Network.Entity;
using Fap.Domain.Entity;
using Fap.Foundation;
using Fap.Network.Services;
using System.Threading;

namespace Fap.Domain.Services
{
    public class DownloadWorkerService
    {
        private Node node;
        private BackgroundSafeObservable<DownloadRequest> queue = new BackgroundSafeObservable<DownloadRequest>();
        private ConnectionService service;
        private NetworkSpeedMeasurement netSpeed = new NetworkSpeedMeasurement();

        private ReaderWriterLockSlim sync = new ReaderWriterLockSlim();
        private string status;


        public DownloadWorkerService(ConnectionService s, Node n, DownloadRequest r)
        {
            node = n;
            service = s;
            status = "Connecting..";
            ThreadPool.QueueUserWorkItem(new WaitCallback(process));
        }

        public void Stop()
        {

        }

        public string Status
        {
            get
            {
                sync.EnterReadLock();
                string status = this.status;
                sync.ExitReadLock();
                return status;
            }
        }

        public long Speed
        {
            get 
            {
                sync.EnterReadLock();
                long speed = netSpeed.GetSpeed();
                sync.ExitReadLock();
                return speed;
            }
        }

        public int Percent
        {
            get { return 0; }
        }

        public int Size
        {
            get { return 0; }
        }

        public bool Completed
        {
            get
            {
                sync.EnterReadLock();
                bool count = queue.Count() == 0;
                sync.ExitReadLock();
                return count;
            }
        }

        public bool IsBusy
        {
            get
            {
                var list = queue.ToList();
                if (queue.Count > 5)
                    return true;
                return list.Select(i => i.Size).Sum() > 22428800;//20mb
            }
        }

        public void AddDownload(DownloadRequest req)
        {
            sync.EnterWriteLock();
            queue.Add(req);
            sync.ExitWriteLock();
        }

        public Node Node
        {
            get { return node; }
        }
        public int Downloads
        {
            get { return queue.Count; }
        }

        private void process(object o)
        {
            //DownloadClient
        }

    }
}
