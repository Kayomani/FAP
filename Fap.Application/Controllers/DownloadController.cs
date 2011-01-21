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
using Fap.Domain.Entity;
using System.Threading;
using Fap.Foundation;
using Fap.Domain.Services;
using Fap.Network.Entity;
using Fap.Network.Services;

namespace Fap.Application.Controllers
{
    public class DownloadController
    {
        private Model model;
        private ReaderWriterLockSlim sync = new ReaderWriterLockSlim();
        private long lastRun = Environment.TickCount;
        private BackgroundSafeObservable<DownloadWorkerService> workers = new BackgroundSafeObservable<DownloadWorkerService>();
        private ConnectionService connectionService;
        private BufferService bufferService;

        public DownloadController(ConnectionService cs, Model m, BufferService bufferService)
        {
            model = m;
            connectionService = cs;
            this.bufferService = bufferService;
        }

        public void Start()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(process));
        }

        private void process(object o)
        {
            while (true)
            {
                if (Environment.TickCount - lastRun > 2000)
                {
                    ScanForDownloads();
                    lastRun = Environment.TickCount;
                }
                else
                {
                    Thread.Sleep(510);
                }
            }
        }


        public void ScanForDownloads()
        {
            sync.EnterWriteLock();
            Console.WriteLine("Checking downloads..");

            if (workers.Where(w=>w.IsBusy).Count() < model.MaxDownloads)
            {
                var downloads =
                from download in model.DownloadQueue.List.ToList()
                group download by download.ClientID into g
                select new
                {
                    Downloads = g,
                    ID = g.First().ClientID,
                };

                foreach (var group in downloads)
                {
                    //Check if the client is online
                    Node client = model.Peers.ToList().Where(p => p.ID == group.ID).FirstOrDefault();
                    if (null == client)
                        client = model.Peers.ToList().Where(c => c.Nickname == group.Downloads.First().Nickname).FirstOrDefault();

                    if (null != client)
                    {
                        var workerlist = workers.Where(w => w.Node == client).ToList();

                        foreach (var item in group.Downloads)
                        {
                            if (item.State == DownloadRequestState.None && item.NextTryTime < Environment.TickCount)
                            {
                                bool addedDownload = false;
                                //Try to place item with a worker
                                for (int i = workerlist.Count; i < model.MaxDownloadsPerUser; i++)
                                {
                                    if (i <= workerlist.Count)
                                    {
                                        //Add a new worker
                                        var worker = new DownloadWorkerService(model, connectionService, client, item, bufferService);
                                        workers.Add(worker);
                                        model.TransferSessions.Add(new TransferSession(worker) { Status = "Connecting..", User = client.Nickname, Size = item.Size, IsDownload = true });
                                        addedDownload = true;
                                        break;
                                    }
                                    else
                                    {
                                        //Is the worker busy? if no give it the request
                                        if (!workers[i].IsBusy)
                                        {
                                            workers[i].AddDownload(item);
                                            addedDownload = true;
                                            break;
                                        }
                                    }
                                }
                                if (!addedDownload)
                                {
                                    //Could not place the download so skip the rest of the queue for this host
                                    break;
                                }
                            }
                        }
                    }
                }
                //Remove redundant workers
                foreach (var worker in workers.Where(w => w.Completed).ToList())
                {
                    workers.Remove(worker);
                    worker.Stop();
                }
            }
            sync.ExitWriteLock();
        }
    }
}
