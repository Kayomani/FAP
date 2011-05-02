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
using System.Text;
using FAP.Domain.Entities;
using System.Threading;
using FAP.Domain.Net;
using FAP.Domain.Services;
using FAP.Domain;

namespace FAP.Application.Controllers
{
    public class WatchdogController
    {
        private bool run =false;

        private Model model;
        private SharesController shareController;
        private List<DownloadWorkerService> workers = new List<DownloadWorkerService>();
        //Sync object for scanfordownloads - only single invocations allowed.
        private object sync = new object();

        public WatchdogController(Model m, SharesController s)
        {
            model = m;
            shareController = s;
        }

        public void Start()
        {
            if (!run)
            {
                run = true;
                ThreadPool.QueueUserWorkItem(new WaitCallback(processCheck));
            }
        }

        private void processCheck(object o)
        {
            long runCount = 0;
            int lastRun = Environment.TickCount;

            while (run)
            {
                lastRun = Environment.TickCount;

                //Update node transfer info - Every 4 seconds
                try
                {
                    model.LocalNode.DownloadSpeed = NetworkSpeedMeasurement.TotalDownload.GetSpeed();
                    model.LocalNode.UploadSpeed = NetworkSpeedMeasurement.TotalUpload.GetSpeed();
                }
                catch { }

                //Poke share controller to check if shares need updating every 5 minutes
                if (runCount % 60 == 0)
                {
                    shareController.RefreshShareInfo();
                }

                //Save config and download queue every 5 minutes but not on start up
                if (runCount != 0 && runCount % 60 == 0)
                {
                    try
                    {
                        model.BlockShutdown = true;
                        model.Save();
                        model.DownloadQueue.Save();
                    }
                    catch { }
                    finally
                    {
                        model.BlockShutdown = false;
                    }
                }

                //Scan for downloads
                try
                {
                    ScanForDownloads();
                }
                catch { }

                //Every 20 seconds try to reduce memory usage
                if (runCount % 4 == 0)
                {
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    System.GC.Collect();
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
                    Node client = model.Network.Nodes.ToList().Where(p => p.ID == group.ID).FirstOrDefault();
                    if (null == client)
                        client = model.Network.Nodes.ToList().Where(c => c.Nickname == group.Downloads.First().Nickname).FirstOrDefault();

                    if (null != client)
                    {
                        var workerlist = workers.Where(w => w.Node == client).ToList();
                        var currentDownloads = group.Downloads.Where(d => d.State != DownloadRequestState.None && d.State != DownloadRequestState.Downloaded).Count();
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
                                        logger.Info("Added download to new downloader");
                                        //Add a new worker
                                        var worker = new DownloadWorkerService(model, connectionService, client, item, bufferService, item);
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
                                            logger.Info("Added download to existing downloader");
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
                        worker.OnDownloaderFinished -= new DownloadWorkerService.DownloaderFinished(worker_OnDownloaderFinished);
                        worker.Stop();
                    }
                }
            }
        }
    }
}
