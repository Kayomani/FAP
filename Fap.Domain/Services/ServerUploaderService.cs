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
using System.Net.Sockets;
using Fap.Domain.Entity;
using Fap.Foundation.Logging;
using System.Threading;
using Fap.Network.Services;
using Fap.Network;
using Fap.Domain.Verbs;
using System.IO;
using Fap.Foundation;
using System.Net;

namespace Fap.Domain.Services
{
    public class ServerUploaderService : ITransferWorker
    {
        private Model model;
        private Logger logger;
        private BufferService bufferService;
        private ServerUploadLimiterService limiterService;
        private NetworkSpeedMeasurement msm;

        private bool isComplete = false;
        private long length = 0;
        private long position = 0;
        private string status = string.Empty;

        public ServerUploaderService(Model m, Logger l, BufferService b, ServerUploadLimiterService limiterService)
        {
            model = m;
            logger = l;
            bufferService = b;
            msm = new NetworkSpeedMeasurement(NetSpeedType.Upload);
            this.limiterService = limiterService;
        }

        public FAPListenerRequestReturnStatus HandleRequest(Request r, Socket s)
        {
            logger.AddInfo("New downloader for " + r.Param);

            DownloadVerb verb = new DownloadVerb();
            verb.ProcessRequest(r);
            MemoryBuffer buffer = null;
            ServerUploadToken token = null;
            s.Blocking = true;
            string remoteHost = (s.RemoteEndPoint as IPEndPoint).Address.ToString();
            Node peer = model.Peers.Where(w => w.Host == remoteHost).FirstOrDefault();
            
            TransferSession session = new TransferSession(this);
            session.IsDownload = false;
            if (null == peer)
                session.User = remoteHost;
            else
                session.User = peer.Nickname;
            model.TransferSessions.Add(session);

            if (s.SendBufferSize != BufferService.Buffer)
                s.SendBufferSize = BufferService.Buffer;
 
            try
            {
                string path = string.Empty;
                if (ReplacePath(verb.Path, out path))
                {
                    using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        verb.FileSize = stream.Length - verb.ResumePoint;

                        length = stream.Length;
                        position = verb.ResumePoint;
                       

                        if(verb.ResumePoint!=0)
                            stream.Seek(verb.ResumePoint,SeekOrigin.Begin);

                        //If the file size is less than a buffer then do a direct transfer
                        if (verb.FileSize < BufferService.SmallBuffer)
                        {
                            buffer = bufferService.GetSmallArg();
                            s.Send(Mediator.Serialize(verb.CreateResponse()));
                            int rx = stream.Read(buffer.Data, 0, buffer.Data.Length);
                            buffer.SetDataLocation(0, rx);
                            if (buffer.DataSize != verb.FileSize)
                            {

                            }
                            status = "Uploading " + Path.GetFileName(verb.Path);
                            session.Size = verb.FileSize;
                            session.Percent = 0;
                            msm.PutData(0);
                            verb.QueuePosition = 0;
                            verb.InQueue = false;
                            //Dont limit files this small
                            s.Send(buffer.Data, 0, buffer.DataSize, SocketFlags.None);
                            session.Percent = 100;
                            msm.PutData(verb.FileSize);
                            session.Speed = msm.GetSpeed();
                        }
                        else if (verb.FileSize < BufferService.Buffer)
                        {
                            buffer = bufferService.GetArg();
                            session.Size = verb.FileSize;
                            session.Percent = 0;
                            
                            //Limit sending the file by slots
                            token = limiterService.RequestUploadToken(peer);
                            long lastQueueStatus = 0;
                            while (!token.CanUpload)
                            {
                                //Send queue info
                                if (Environment.TickCount - lastQueueStatus > 5000)
                                {
                                    verb.InQueue = true;
                                    verb.QueuePosition = token.Position;
                                    status = "Queued in position " + verb.QueuePosition + " "  + Path.GetFileName(verb.Path);
                                    Console.WriteLine("Queue");
                                    s.Send(Mediator.Serialize(verb.CreateResponse()));
                                    lastQueueStatus = Environment.TickCount;
                                }
                                token.Wait();
                            }
                            //Send file header and data
                            verb.QueuePosition = 0;
                            verb.InQueue = false;
                            msm.PutData(0);
                            status = "Uploading " + Path.GetFileName(verb.Path);
                            s.Send(Mediator.Serialize(verb.CreateResponse()));
                            int rx = stream.Read(buffer.Data, 0, buffer.Data.Length);
                            buffer.SetDataLocation(0, rx);
                            s.Send(buffer.Data, 0, buffer.DataSize, SocketFlags.None);
                            session.Percent = 100;
                            msm.PutData(verb.FileSize);
                            session.Speed = msm.GetSpeed();
                        }
                        else
                        {
                            session.Size = verb.FileSize;
                            session.Percent = 0;
                            //Limit sending the file by slots
                            token = limiterService.RequestUploadToken(peer);
                            long lastQueueStatus = 0;
                            while (!token.CanUpload)
                            {
                                //Send queue info
                                if (Environment.TickCount - lastQueueStatus > 5000)
                                {
                                    verb.InQueue = true;
                                    verb.QueuePosition = token.Position;
                                    status = "Queued in position " + verb.QueuePosition + " " + Path.GetFileName(verb.Path);
                                    s.Send(Mediator.Serialize(verb.CreateResponse()));
                                }
                                token.Wait();
                            }
                            status = "Uploading " + Path.GetFileName(verb.Path);
                            //Send file header and data
                            verb.QueuePosition = 0;
                            verb.InQueue = false;
                            s.Send(Mediator.Serialize(verb.CreateResponse()));

                            //We have a large file so read the file on another thread to try to improve performance
                            using (BufferedFileReader bfr = new BufferedFileReader(bufferService))
                            {
                                try
                                {
                                    bfr.Start(stream);
                                    while (!bfr.IsEOF)
                                    {
                                        buffer = bfr.GetBuffer();
                                        s.Send(buffer.Data,buffer.StartLocation, buffer.DataSize, SocketFlags.None);
                                        msm.PutData(buffer.DataSize);
                                        session.Speed = msm.GetSpeed();
                                        bufferService.FreeArg(buffer);
                                        if (bfr.HasError)
                                        {
                                            s.Close();
                                            return FAPListenerRequestReturnStatus.None;
                                        }
                                    }
                                }
                                catch { }
                            }
                            buffer = null;
                        }
                        stream.Close();
                    }
                }
                else
                {
                    verb.Error = true;
                    s.Send(Mediator.Serialize(verb.CreateResponse()));
                }
                status = "Upload complete for " + Path.GetFileName(verb.Path);
            }
            catch(Exception e)
            {
                //File doesnt exist or other
                verb.Error = true;
                //If still connected transmit error
                if (s.Connected)
                {
                    s.Blocking = false;
                    s.Send(Mediator.Serialize(verb.CreateResponse()));
                    s.Blocking = true;
                }
                status = "Upload error: " + e.Message;
            }
            finally
            {
                if (null != buffer)
                    bufferService.FreeArg(buffer);
                if (null != token)
                    limiterService.FreeToken(token);
                model.TransferSessions.Remove(session);
                isComplete = true;

            }
            return FAPListenerRequestReturnStatus.Disposed;
        }



        private bool ReplacePath(string input, out string output)
        {
            string[] split = input.Split('\\');
            if (split.Length > 0)
            {
                var share = model.Shares.Where(s => s.Name == split[0]).FirstOrDefault();
                if (null != share)
                {
                    output = share.Path + "\\" + input.Substring(share.Name.Length, input.Length - share.Name.Length);
                    return true;
                }
            }
            output = string.Empty;
            return false;
        }

        public long Length
        {
            get 
            {
                return length;
            }
        }

        public bool IsComplete
        {
            get 
            {
                return isComplete;
            }
        }

        public long Speed
        {
            get 
            {
                return msm.GetSpeed();
            }
        }

        public string Status
        {
            get 
            {
                return status;
            }
        }

        public long Position
        {
            get 
            {
                return position;
            }
        }
    }
}
