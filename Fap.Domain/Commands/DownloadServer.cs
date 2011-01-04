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
using System.Net.Sockets;
using Fap.Domain.Services;
using System.IO;
using System.Threading;
using Fap.Foundation;
using Fap.Network.Entity;
using Fap.Foundation.Logging;
using Fap.Network.Services;
using Fap.Network;

namespace Fap.Domain.Commands
{
    class DownloadServer
    {
        private Model model;
        private Mediator mediator;
        private BufferService bufferService;
        private ServerUploadLimiterService limiterService;
        private Logger logService;

        public DownloadServer(Model model, BufferService bs, ServerUploadLimiterService limiter, Logger log)
        {
            mediator = new Mediator();
            this.model = model;
            this.bufferService = bs;
            this.limiterService = limiter;
            this.logService = log;
        }


        private bool ReplacePath(List<Share> shares, string input, out string output)
        {
            string[] split = input.Split('\\');
            if (split.Length > 0)
            {
                var share = shares.Where(s => s.Name == split[0]).FirstOrDefault();
                if (null != share)
                {
                    output = share.Path + "\\" + input.Substring(share.Name.Length, input.Length - share.Name.Length);
                    return true;
                }
            }
            output = string.Empty;
            return false;
        }



        public void ProcessRequest(MemoryBuffer reqArg, string[] inc, Session session)
        {
            string[] ret = new string[3];
            string response = string.Empty;
            long resumepoint = 0;
            ret[0] = "DOWNLOAD";
            List<Share> shareList;
            lock (model.Shares)
            {
                shareList = model.Shares.ToList();
            }
            if (inc.Length > 2)
            {
                string filePath = inc[1];
                if (ReplacePath(shareList, filePath, out filePath))
                {
                    FileInfo info = new FileInfo(filePath);
                    if (info.Exists)
                    {
                        Fap.Domain.Services.ServerUploadLimiterService.ServerUploadToken token;
                        string fileName = Path.GetFileName(inc[1]);
                        logService.AddInfo("DL Server rx File " + fileName);
                       /* if (!limiterService.RequestUploadToken(out token, session.Host))
                        {
                            int wait = 20;
                            //We don't have any slots left so report status and wait.
                            while (!token.AllowedToUpload)
                            {
                                logService.AddInfo("DL Server Queued File " + fileName);
                                //Report
                                ret[1] = "QUEUE";
                                ret[2] = token.Position.ToString();
                                response = Mediator.Serialise(ret);
                                reqArg.Socket.Send(ASCIIEncoding.ASCII.GetBytes(response));
                                session.Status = "Queued upload - Position:" + token.Position + " for " + fileName;
                                //Wait
                               // if (token.AllowedToLock)
                                //    token.Wait();
                               // else
                                if (wait < 120)
                                    wait += 20;
                                Thread.Sleep(wait);
                            }
                        }*/

                        BufferedFileReader bfr = new BufferedFileReader(bufferService);
                        resumepoint = long.Parse(inc[2]);
                        bfr.Start(filePath,resumepoint);

                        int waittime = 5;
                        while (bfr.readbuffer.Count == 0 && !bfr.IsEOF)
                        {
                            Thread.Sleep(waittime);
                            if (waittime < 50)
                                waittime += 5;

                        }

                        if (bfr.IsEOF && bfr.HasError)
                        {
                            session.Status = "Error reading:" + fileName;
                            //Error reading file
                            ret[1] = "ERROR";
                            ret[2] = string.Empty;
                           // response = Mediator.Serialise(ret);
                            reqArg.Socket.Send(ASCIIEncoding.ASCII.GetBytes(response));
                        }
                        else
                        {
                            //Ok send file
                            ret[1] = "OK";
                            ret[2] = info.Length.ToString();
                           // response = Mediator.Serialise(ret);
                            reqArg.Socket.Send(ASCIIEncoding.ASCII.GetBytes(response));
                            logService.AddInfo("DL Server OK File " + fileName);

                            string path = inc[1].Substring(0, inc[1].Length - fileName.Length);
                            session.Status = "Download " + fileName + " in " + path;
                            session.Length = info.Length;

                            ProcessSendSync(bfr, reqArg, session, info.Length, resumepoint);
                        }
                       // limiterService.FreeToken(token);
                        logService.AddInfo("DL Server freed token");
                        return;
                    }
                }
            }
            ret[1] = "DOWNLOAD";
            ret[2] = "ERROR";
           // response = Mediator.Serialise(ret);
            reqArg.Socket.Send(ASCIIEncoding.ASCII.GetBytes(response));
        }



        private void ProcessSendSync(BufferedFileReader info, MemoryBuffer e, Session session, long length,long resume)
        {
            NetworkSpeedMeasurement nsm = new NetworkSpeedMeasurement();
            long dataSent = resume;

            int wait = 5;
            long tx = 0;
            while (!(info.IsEOF && info.readbuffer.Count==0))
            {
                if (info.readbuffer.Count > 0)
                {
                    wait = 5;
                    var buf = info.readbuffer.Dequeue();
                    if (e.Socket.SendBufferSize != buf.Data.Length)
                        e.Socket.SendBufferSize = buf.Data.Length;
                    int sent = e.Socket.Send(buf.Data, 0, buf.DataSize, SocketFlags.None);
                     dataSent += sent;
                     tx += sent;
                     session.Transfered = dataSent;
                     nsm.PutData(sent);
                     session.Speed = nsm.GetSpeed();
                     bufferService.FreeArg(buf);
                }
                else
                {
                    Thread.Sleep(wait);
                    if (wait < 100)
                        wait += 10;
                }
            }
            if (info.HasError)
            {

                try
                {
                    if (session.Socket.Connected)
                    {
                        session.Socket.Shutdown(SocketShutdown.Both);
                        session.Socket.Close();
                    }
                }
                catch (Exception ex)
                {
                    logService.LogException(ex);
                }
            }
            logService.AddInfo("DL Server Send complete File " +dataSent);
           // e.AcceptSocket.Close();
          //  bufferService.FreeArg(e);
        }

       // private static long dataSent = 0;

       /* void ProcessSend(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
        {
          //  Console.WriteLine("+Send Begin");
            BufferedFileReader info = e.UserToken as BufferedFileReader;

          //  Console.WriteLine(e.SocketError);
            if (info.LastBuffer != null)
                bufferService.FreeArg(info.LastBuffer);
            bool done = info.IsEOF;
            //Make sure we send the last block
            if(done)
                done = (info.readbuffer.Count==0);

            if (!done)
            {
                bool retry = false;
                lock (info.readbuffer)
                {
                    if (info.readbuffer.Count > 0)
                    {
                        var data = info.readbuffer.Dequeue();
                        if (e.AcceptSocket.SendBufferSize != data.Buffer.Length)
                            e.AcceptSocket.SendBufferSize = data.Buffer.Length;

                        info.LastBuffer = data;
                        e.SetBuffer(data.Buffer, 0, data.DataSize);
                        
                      //  Console.WriteLine("TX " + info.readbuffer.Count + " " + e.Buffer[0]);
                        if (!e.AcceptSocket.SendAsync(e))
                            ProcessSend(sender, e);
                    }
                    else
                    {
                        retry = true;
                    }
                }

                if (!e.AcceptSocket.Connected)
                {
                    info.IsEOF = true;
                    Console.WriteLine("Send Closed");
                    return;
                }

                if (retry)
                {
                    while(info.readbuffer.Count==0)
                      Thread.Sleep(5);
                    ProcessSend(null, e);
                }

            }
            else
            {
                Console.WriteLine("Send Complete ");
                e.Completed -= new EventHandler<System.Net.Sockets.SocketAsyncEventArgs>(ProcessSend);
                //e.AcceptSocket.Close();
                bufferService.FreeArg(e);
            }
        }*/
    }
}
