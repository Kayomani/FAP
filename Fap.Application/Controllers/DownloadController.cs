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
using Fap.Foundation.Logging;

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
        private Logger logger;

        public DownloadController(ConnectionService cs, Model m, BufferService bufferService, Logger logger)
        {
            model = m;
            connectionService = cs;
            this.bufferService = bufferService;
            this.logger = logger;
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
          //  Console.WriteLine("Checking downloads..");

            foreach (var worker in workers.ToList())
            {
                if (worker.IsComplete)
                    workers.Remove(worker);
            }

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
                    var currentDownloads = group.Downloads.Where(d => d.State != DownloadRequestState.None && d.State!=DownloadRequestState.Downloaded).Count();
                    foreach (var item in group.Downloads)
                    {
                        if (workers.Count > model.MaxDownloads)
                            break;

                        if (item.State == DownloadRequestState.Downloaded)
                        {
                            model.DownloadQueue.List.Remove(item);
                        }
                        else if (item.State == DownloadRequestState.None && item.NextTryTime < Environment.TickCount)
                        {
                            bool addedDownload = false;
                            //Try to place item with a worker

                            for (int i = currentDownloads; i < model.MaxDownloadsPerUser; i++)
                            {
                                if (i >= workerlist.Count)
                                {
                                    logger.AddInfo("Added download to new downloader");
                                    //Add a new worker
                                    var worker = new DownloadWorkerService(model, connectionService, client, item, bufferService, item,logger);
                                    worker.OnDownloaderFinished += new DownloadWorkerService.DownloaderFinished(worker_OnDownloaderFinished);
                                    workers.Add(worker);
                                    model.TransferSessions.Add(new TransferSession(worker) { Status = "Connecting..", User = client.Nickname, Size = item.Size, IsDownload = true });
                                    workerlist.Add(worker);
                                    addedDownload = true;
                                    break;
                                }
                                else
                                {
                                    //Is the worker busy? if no give it the request
                                    if (!workerlist[i].IsBusy)
                                    {
                                        logger.AddInfo("Added download to existing downloader");
                                        workerlist[i].AddDownload(item);
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
                //Remove redundant workers
                foreach (var worker in workers.Where(w => w.IsComplete).ToList())
                {
                    workers.Remove(worker);
                    worker.OnDownloaderFinished-=new DownloadWorkerService.DownloaderFinished(worker_OnDownloaderFinished);
                    worker.Stop();
                }
            }
            sync.ExitWriteLock();
        }

        private void worker_OnDownloaderFinished()
        {
            ScanForDownloads();
        }
    }
}
