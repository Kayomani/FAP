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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FAP.Domain;
using FAP.Domain.Entities;
using FAP.Domain.Net;
using FAP.Domain.Services;
using NLog;

namespace FAP.Application.Controllers
{
    public class WatchdogController
    {
        private readonly BufferService bufferService;
        private readonly Model model;
        private readonly OverlordManagerService overlordLauncherService;
        private readonly SharesController shareController;
        //Sync object for scanfordownloads - only single invocations allowed.
        private readonly object sync = new object();
        private readonly List<DownloadWorkerService> workers = new List<DownloadWorkerService>();
        private readonly DokanController dokanController;
        private Logger logger;
        private bool run;

        public WatchdogController(Model m, SharesController s, BufferService b, OverlordManagerService o,DokanController d)
        {
            model = m;
            shareController = s;
            bufferService = b;
            logger = LogManager.GetLogger("faplog");
            overlordLauncherService = o;
            dokanController = d;
        }

        public void Start()
        {
            if (!run)
            {
                run = true;
                ThreadPool.QueueUserWorkItem(processCheck);
            }
        }

        public void Stop()
        {
            run = false;
        }

        private void processCheck(object o)
        {
            long runCount = 0;
            int lastRun = Environment.TickCount;

            while (run)
            {
                lastRun = Environment.TickCount;

                //Check to see if we need to launch an overlord
                overlordLauncherService.StartAndStopIfNeeded();

                //Update node transfer info - Every 4 seconds
                try
                {
                    model.LocalNode.DownloadSpeed = NetworkSpeedMeasurement.TotalDownload.GetSpeed();
                    model.LocalNode.UploadSpeed = NetworkSpeedMeasurement.TotalUpload.GetSpeed();
                }
                catch
                {
                }

                bufferService.Clean();

                //Poke share controller to check if shares need updating every 5 minutes
                if (runCount%60 == 0)
                {
                    shareController.RefreshShareInfo();
                }

                //Save config and download queue every 5 minutes but not on start up
                if (runCount != 0 && runCount%60 == 0)
                {
                    try
                    {
                        model.GetAntiShutdownLock();
                        model.Save();
                        model.DownloadQueue.Save();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        model.ReleaseAntiShutdownLock();
                    }
                }

                //Scan for downloads
                try
                {
                    ScanForDownloads();
                }
                catch
                {
                }

                //Every 20 seconds try to reduce memory usage
                if (runCount%4 == 0)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    //Clean dokan cache
                    dokanController.CleanUp();
                }

                //Wait 5 seconds minus the time it took to execute
                int wait = 5000 - (Environment.TickCount - lastRun);
                if (wait > 0)
                    Thread.Sleep(wait);
                runCount++;
            }
        }


        private void ScanForDownloads()
        {
            lock (sync)
            {
                foreach (DownloadRequest item in model.DownloadQueue.List.ToList())
                {
                    switch (item.State)
                    {
                        case DownloadRequestState.Downloaded:
                            //Remove completed items if they have some how leaked in, this should never occur.
                            model.DownloadQueue.List.Remove(item);
                            break;
                        case DownloadRequestState.Error:
                            //Set items to retry
                            if (item.NextTryTime < Environment.TickCount)
                                item.State = DownloadRequestState.None;
                            break;
                    }
                }

                var downloads =
                    from download in model.DownloadQueue.List.ToList().Where(d => d.State == DownloadRequestState.None)
                    group download by download.ClientID
                    into g
                    select new
                               {
                                   Downloads = g,
                                   ID = g.First().ClientID,
                               };

                foreach (var group in downloads)
                {
                    //Check if the client is online
                    Node client = model.Network.Nodes.ToList().Where(p => p.ID == group.ID).FirstOrDefault();
                    if (null == client)
                        client =
                            model.Network.Nodes.ToList().Where(c => c.Nickname == group.Downloads.First().Nickname).
                                FirstOrDefault();

                    if (null != client)
                    {
                        foreach (DownloadRequest item in group.Downloads)
                        {
                            if (item.State == DownloadRequestState.None)
                            {
                                bool addedDownload = false;

                                if (workers.Where(w => w.Node == client).Count() < model.MaxDownloadsPerUser &&
                                    workers.Count < model.MaxDownloads)
                                {
                                    addedDownload = true;
                                    //Max workers not reached, add download via new worker.
                                    var worker = new DownloadWorkerService(client, model, bufferService);
                                    worker.OnWorkerFinished += worker_OnWorkerFinished;
                                    workers.Add(worker);
                                    worker.AddDownload(item);
                                    model.TransferSessions.Add(new TransferSession(worker)
                                                                   {
                                                                       Status = "Connecting..",
                                                                       User = client.Nickname,
                                                                       Size = item.Size,
                                                                       IsDownload = true
                                                                   });
                                }
                                else
                                {
                                    //Max downloaders reached, try to add to an existing queue
                                    foreach (DownloadWorkerService worker in workers.Where(w => w.Node == client))
                                    {
                                        if (!worker.IsQueueFull)
                                        {
                                            worker.AddDownload(item);
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
                foreach (DownloadWorkerService worker in workers.Where(w => w.IsComplete).ToList())
                {
                    worker.OnWorkerFinished -= worker_OnWorkerFinished;
                    workers.Remove(worker);
                    TransferSession session =
                        model.TransferSessions.ToList().Where(t => t.Worker == worker).FirstOrDefault();
                    if (null != session)
                        model.TransferSessions.Remove(session);
                }
            }
        }

        private void worker_OnWorkerFinished(object sender, EventArgs e)
        {
            ScanForDownloads();
        }
    }
}