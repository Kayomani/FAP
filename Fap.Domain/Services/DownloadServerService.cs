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
    public class DownloadServerService
    {
        private Model model;
        private Logger logger;
        private BufferService bufferService;
        private ServerUploadLimiterService limiterService;
        private NetworkSpeedMeasurement msm;

        public DownloadServerService(Model m, Logger l, BufferService b, ServerUploadLimiterService limiterService)
        {
            model = m;
            logger = l;
            bufferService = b;
            msm = new NetworkSpeedMeasurement();
            this.limiterService = limiterService;
        }

        public bool HandleRequest(Request r, Socket s)
        {
            logger.AddInfo("New downloader for " + r.Param);

            DownloadVerb verb = new DownloadVerb();
            verb.ProcessRequest(r);
            MemoryBuffer buffer = null;
            ServerUploadToken token = null;

            string remoteHost = (s.RemoteEndPoint as IPEndPoint).Address.ToString();
            Node peer = model.Peers.Where(w => w.Host == remoteHost).FirstOrDefault();
            
            TransferSession session = new TransferSession(null);
            session.IsDownload = false;
            if (null == peer)
                session.User = remoteHost;
            else
                session.User = peer.Host;
            model.TransferSessions.Add(session);
 
            try
            {
                string path = string.Empty;
                if (ReplacePath(verb.Path, out path))
                {
                    using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        verb.FileSize = stream.Length - verb.ResumePoint;

                        if(verb.ResumePoint!=0)
                            stream.Seek(verb.ResumePoint,SeekOrigin.Begin);

                        //If the file size is less than a buffer then do a direct transfer
                        if (verb.FileSize < BufferService.SmallBuffer)
                        {
                            buffer = bufferService.GetSmallArg();
                            s.Send(Mediator.Serialize(verb.CreateResponse()));
                            buffer.DataSize = stream.Read(buffer.Data, 0, buffer.Data.Length);
                            session.Size = verb.FileSize;
                            session.Percent = 0;
                            //Dont limit files this small
                            s.Send(buffer.Data, 0, buffer.DataSize, SocketFlags.None);
                            session.Percent = 100;
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
                                    s.Send(Mediator.Serialize(verb.CreateResponse()));
                                }
                                token.Wait();
                            }
                            //Send file header and data
                            verb.QueuePosition = 0;
                            verb.InQueue = false;
                            s.Send(Mediator.Serialize(verb.CreateResponse()));
                            buffer.DataSize = stream.Read(buffer.Data, 0, buffer.Data.Length);
                            s.Send(buffer.Data, 0, buffer.DataSize, SocketFlags.None);
                            session.Percent = 100;
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
                                    s.Send(Mediator.Serialize(verb.CreateResponse()));
                                }
                                token.Wait();
                            }
                            //Send file header and data
                            verb.QueuePosition = 0;
                            verb.InQueue = false;
                            s.Send(Mediator.Serialize(verb.CreateResponse()));

                            //We have a large file so read the file on another thread to try to improve performance
                            BufferedFileReader bfr = new BufferedFileReader(bufferService);
                            bfr.Start(stream);

                            while (!bfr.IsEOF)
                            {
                                buffer = bfr.GetBuffer();
                                s.Send(buffer.Data, buffer.DataSize, SocketFlags.None);
                                bufferService.FreeArg(buffer);
                                if (bfr.HasError)
                                {
                                    s.Close();
                                    return false;
                                }
                            }
                        }
                        stream.Close();
                    }
                }
                else
                {
                    verb.Error = true;
                    s.Send(Mediator.Serialize(verb.CreateResponse()));
                }

            }
            catch
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
            }
            finally
            {
                if (null != buffer)
                    bufferService.FreeArg(buffer);
                if (null != token)
                    limiterService.FreeToken(token);
                model.TransferSessions.Remove(session);
            }
            return true;
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
    }
}
