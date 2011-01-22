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
using Fap.Network.Services;
using Fap.Network.Entity;
using Fap.Foundation.Logging;

namespace Fap.Domain.Services
{
    /*public class DownloadService 
    {
        private Model model;
        private Timer timer;
        private delegate void StartProcess(object o);
        private event StartProcess startProcess;
        ReaderWriterLockSlim sync = new ReaderWriterLockSlim();
        private ClientDownloadLimiterService limiter;
        private Logger logger;
        private BufferService bufferService;
        private ConnectionService cs;

        private delegate void StartDownloadAsync(DownloadRequest request, RemoteClient client);

        public DownloadService(Model model, ClientDownloadLimiterService limiter, Logger log, BufferService bufferService, ConnectionService cs)
        {
            this.model = model;
            this.limiter = limiter;
            startProcess = new StartProcess(process);
            logger = log;
            this.bufferService = bufferService;
            this.cs = cs;
        }

        private void DownloadService_OnStartDownloadAsync(DownloadRequest download, RemoteClient client)
        {
            DownloadClient cmd = null;
            try
            {
                cmd = new DownloadClient(model,bufferService,cs,logger);//We sometimes get compsoition errors :O
                cmd.StartDownload(download, client);
                cmd.FreeSession();
            }
            catch (Exception e)
            {
                download.Status = "File not availible";
                download.NextTryTime = Environment.TickCount +(1000* 30);
                model.DownloadQueue.List.Add(download);
                logger.LogException(e);
                cmd.FreeSession();
            }
            sync.EnterWriteLock();
            limiter.FreeToken(client);
            sync.ExitWriteLock();
            //Start the next download 
            process(null);

        }

        public void Run()
        {
            timer = new Timer(new System.Threading.TimerCallback(process), null, 0, 5000);
        }

        public void Stop()
        {
            timer.Dispose();
        }

        public void StartCheck()
        {
            startProcess.BeginInvoke(null,null, null);
        }


        private void processAsync(IAsyncResult a)
        {
            process(null);
        }

        private void process(object o)
        {
            sync.EnterWriteLock();
            Console.WriteLine("Checking downloads..");

            //Create download groups
            var downloads =
                 from download in model.DownloadQueue.List.ToList()
                 group download by download.Host into g
                 select new
                 {
                     Downloads = g,
                     Host = g.FirstOrDefault().Host
                 };
            //Check each group for availible downloads
            foreach (var group in downloads)
            {
                //Check if the client is online
                var client = model.Peers.ToList().Where(c => c.Nickname == group.Host ).FirstOrDefault();
                if (null != client)
                {
                    foreach (var download in group.Downloads)
                    {
                        if (download.NextTryTime < Environment.TickCount)
                        {
                            //Check if we have exceeded the download connection limit
                           /* if (limiter.GetDownloadToken(client))
                            {
                                //Start download async
                                model.DownloadQueue.List.Remove(download);
                                StartDownloadAsync del = new StartDownloadAsync(DownloadService_OnStartDownloadAsync);
                                del.BeginInvoke(download, client, null, null);
                                //OnStartDownloadAsync.BeginInvoke(download, client,DownloadService_OnStartDownloadAsync,null);
                            }
                            else
                                break;*//*
                        }
                    }
                }
            }
            sync.ExitWriteLock();
        }
    }*/
}
