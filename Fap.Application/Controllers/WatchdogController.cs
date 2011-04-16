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
using System.Threading;
using Fap.Domain.Services;
using Fap.Domain.Entity;
using Fap.Network.Services;
using Fap.Network.Entity;
using Fap.Foundation;
using Fap.Domain.Verbs;
using Fap.Network;
using System.Net.Sockets;
using NLog;

namespace Fap.Application.Controllers
{
    public class WatchdogController
    {
        private ConnectionService connectionService;
        private Model model;
        private Logger logger;
        private BufferService bufferService;
        private Thread worker;
        private LANPeerConnectionService peerService;
        private UplinkConnectionPoolService ucps;
        private SharesController shareController;

        public WatchdogController(ConnectionService cs, Model model, BufferService bufferService, LANPeerConnectionService ps, UplinkConnectionPoolService u, SharesController shareController)
        {
            connectionService = cs;
            this.model = model;
            logger = LogManager.GetLogger("faplog");
            this.bufferService = bufferService;
            peerService = ps;
            ucps = u;
            this.shareController = shareController;
        }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(processCheck));
        }

        public void Stop()
        {
            if (worker != null)
                worker.Abort();
        }


        private void processCheck(object o)
        {
            worker = Thread.CurrentThread;
            Thread.CurrentThread.IsBackground = false;

            int speedCount = 0;
            long lastSave = Environment.TickCount;

            while (true)
            {
                try
                {
                    //Disconnect sessions if needed
                  //  DisconnectStaleSessions();//Temp disabled as ping is causing an error??
                    //Clean up excess buffers
                    bufferService.Clean();

                    //Remove completed download sessions from view
                    foreach (var session in model.TransferSessions.ToList())
                    {
                        if (null != session.Worker && session.Worker.IsComplete)
                            model.TransferSessions.Remove(session);
                    }
                    //Update transfer stats every 4 seconds
                    if (speedCount > 3)
                    {
                        speedCount = 0;
                        //Check for disconnected server connections
                        model.Node.DownloadSpeed = NetworkSpeedMeasurement.TotalDownload.GetSpeed();
                        model.Node.UploadSpeed = NetworkSpeedMeasurement.TotalUpload.GetSpeed();

                        shareController.RefreshShareInfo();
                    }
                    else
                        speedCount++;

                    //Save config + queue every 5 minutes
                    if (Environment.TickCount - lastSave > 1000 * 300)
                    {
                        lastSave = Environment.TickCount;
                        model.Save();
                        model.DownloadQueue.Save();
                    }
                    // Remove any old pooled server connections.
                    ucps.CleanUp();
                }
                catch { }
                Thread.Sleep(1000);
            }
        }

        public void DisconnectStaleSessions()
        {
            List<Session> staleSessions = connectionService.GetAndClearStaleSessions();
            foreach (Session session in staleSessions)
            {
                try
                {
                    if (session.Socket.Connected)
                    {
                        //Notify server
                        PingVerb cmd = new PingVerb(model.Node);

                        Request req = cmd.CreateRequest();
                        req.ConnectionClose = true;
                        session.Socket.SendTimeout = 3000;
                        session.Socket.ReceiveTimeout = 3000;
                        session.Socket.Send(Mediator.Serialize(req));
                         var buff = bufferService.GetSmallArg();
                        try
                        {
                            session.Socket.Receive(buff.Data, buff.DataSize, SocketFlags.None);
                        }
                        finally
                        {
                            bufferService.FreeArg(buff);
                        }

                        //Disconnect
                        session.Socket.SendBufferSize = 1;
                        session.Socket.ReceiveBufferSize = 1;
                        session.Socket.Shutdown(SocketShutdown.Both);
                        // session.Socket.Disconnect(true);
                        session.Socket.Close();
                        session.Socket = null;
                    }
                    session.Host = null;
                    session.Socket = null;
                }
                catch { }
            }
        }
    }
}
