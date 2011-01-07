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
using Fap.Foundation.Logging;
using Fap.Network.Entity;
using Fap.Domain.Commands;

namespace Fap.Application.Controllers
{
    public class WatchdogController
    {
      //  private readonly long ClientTimeout = 2*60*1000;//2 minutes

        private ConnectionService connectionService;
        private Model model;
        private Logger logger;
        private BufferService bufferService;
        private Thread worker;

        public WatchdogController(ConnectionService cs, Model model, Logger log, BufferService bufferService)
        {
            connectionService = cs;
            this.model = model;
            logger = log;
            this.bufferService = bufferService;
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

             while(true)
            {
                try
                {
                    {
                        // logger.AddInfo("Processing watchdog");
                        //Disconnect sessions if needed
                        DisconnectStaleSessions();
                        //Update user display info
                        foreach (var session in model.Sessions.ToList())
                        {
                            Node rc = session.Host;
                            if (rc != null)
                            {
                                if (session.User != rc.Nickname && !string.IsNullOrEmpty(rc.Nickname))
                                {
                                    session.User = rc.Nickname;
                                }
                                else if (string.IsNullOrEmpty(rc.Nickname) && session.User != rc.Host)
                                {
                                    session.User = rc.Host;
                                }
                            }
                            else
                            {
                                // rc = model.Clients.Where(c => c.Host == session.User).FirstOrDefault();
                                session.Host = rc;
                            }
                        }
                        //Clean up excess buffers
                        bufferService.Clean();

                        //Check clients are still alive
                        /* foreach (var client in model.Clients.ToList())
                         {
                             if (client.LastAccess + ClientTimeout < Environment.TickCount)
                                 model.Clients.Remove(client);
                         }*/


                        //Check for disconnected server connections
                    }
                }
                catch { }
                Thread.Sleep(1000);
            }
        }

      public void DisconnectStaleSessions()
      {
        /*  try
          {
              List<Session> staleSessions = new List<Session>();
              lock (sync)
              {
                  staleSessions = model.Sessions.Where(s => !s.InUse && s.Stale).ToList();
                  foreach (var session in staleSessions)
                  {
                      session.InUse = true;
                      model.Sessions.Remove(session);
                  }
              }
              foreach (Session session in staleSessions)
              {
                  if (session.Socket.Connected)
                  {
                      //Notify server
                      DisconnectCMD cmd = new DisconnectCMD();
                      string[] data = cmd.CreateRequest(session);
                      Mediator m = new Mediator();
                      //transmit
                      string tx = Mediator.Serialise(data);
                      session.Socket.Send(ASCIIEncoding.Unicode.GetBytes(tx));

                      //Disconnect
                      //session.Socket.SendBufferSize = 1;
                      // session.Socket.ReceiveBufferSize = 1;
                      session.Socket.Shutdown(SocketShutdown.Both);
                      // session.Socket.Disconnect(true);
                      session.Socket.Close();
                      session.Socket = null;
                  }
                  session.Host = null;
                  session.Socket = null;
              }
          }
          catch { }*/
      }
    }
}
