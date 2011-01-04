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
using Fap.Domain.Services;
using System.Net.Sockets;
using Fap.Domain.Entity;
using System.IO;
using System.Threading;
using Fap.Foundation;
using Fap.Network.Services;
using Fap.Network.Entity;
using Fap.Foundation.Logging;
using Fap.Network;

namespace Fap.Domain.Commands
{
    class DownloadClient
    {
        private readonly long RECEIVE_TIMEOUT = 45000;

        private Model model;
        private Mediator mediator;
        private BufferService bufferService;
        private ConnectionService connectionService;
        private Logger logService;

        private Session session;

        public DownloadClient(Model model, BufferService bs, ConnectionService cs, Logger log)
        {
            mediator = new Mediator();
            this.model = model;
            bufferService = bs;
            connectionService = cs;
            logService = log;
        }


        public void StartDownload(DownloadRequest req, RemoteClient rc)
        {
            if (req.IsFolder)
            {
               /* logService.AddInfo("Client download folder: " + req.FullPath);
                // session.Status = "Started get folder info for " + req.FullPath;
                //Download file list and add it to the queue.
                Client c = new Client(bufferService,connectionService);
                BrowseCMD cmd = new BrowseCMD(model, rc);
                cmd.Path = req.FullPath;
               // if (c.Execute(cmd, rc))
                {
                    //Create a new download for each item
                    foreach (var item in cmd.Results)
                    {
                        DownloadRequest dlr = new DownloadRequest()
                        {
                            Added = DateTime.Now,
                            IsFolder = item.IsFolder,
                            Host = rc.Nickname,
                            FullPath = item.FullPath,
                        };

                        //If we are using folders then put stuff in a sub folder
                        if (!string.IsNullOrEmpty(req.LocalPath))
                            dlr.LocalPath = req.LocalPath + "\\" + req.FileName;
                        else
                            dlr.LocalPath = req.FileName;

                        model.DownloadQueue.List.Add(dlr);
                    }

                }*/
                // session.Status = "Completed get folder info for " + req.FullPath;
            }
            else
            {
               // session = connectionService.GetClientSession(rc);
                Token token = new Token();
                var arg = bufferService.GetArg();
                long resumePoint = 0;

                  string path = string.Empty;
                        if (string.IsNullOrEmpty(req.LocalPath))
                            path = model.DownloadFolder + "\\" + Path.GetFileName(req.FullPath);
                        else
                            path = model.DownloadFolder + "\\" + req.LocalPath + "\\" + Path.GetFileName(req.FullPath);
                try
                {
                    // session.Status = "Starting download of " + req.FileName;
                    //Download a file
                    string[] ret = new string[3];
                    ret[0] = "DOWNLOAD";
                    ret[1] = req.FullPath;

                    if (File.Exists(path))
                    {
                        FileInfo fi = new FileInfo(path);
                        ret[2] = fi.Length.ToString();
                        resumePoint = fi.Length;
                    }
                    else
                    {
                        ret[2] = "0";
                    }
                    string request = "";// Mediator.Serialise(ret);
                    session.Status = "Requesting: " + req.FileName;
                    logService.AddInfo("Client download file: " + req.FullPath);
                    session.Socket.ReceiveBufferSize = arg.Data.Length;
                    session.Socket.Send(ASCIIEncoding.ASCII.GetBytes(request));


                    string[] rx2 = new string[0];
                    bool run = true;
                    int wait = 5;
                    long lastReceive = Environment.TickCount;

                    while (run)
                    {
                        bool proc = false;
                        if (session.Socket.Available > 0)
                        {
                            lastReceive = Environment.TickCount;
                            logService.AddInfo("Client download Begin status RX file: " + req.FullPath);
                            arg.SetDataLocation(0, session.Socket.Receive(arg.Data));
                            logService.AddInfo("Client download End status RX file: " + req.FullPath);
                            token.ReceiveData(arg);
                            wait = 5;
                        }

                        while (token.ContainsCommand())
                        {
                            proc = true;
                            rx2 = null;// Mediator.Deserialize(token.GetCommand());
                            if (rx2.Length == 3)
                            {
                                if (rx2[1] == "OK")
                                {
                                    run = false;
                                    break;
                                }
                                else if (rx2[1] == "QUEUE")
                                {
                                    session.Status = "Download queued in position " + rx2[2];
                                    logService.AddInfo("Client download (" + req.FileName + "): Got QUEUE");
                                }
                                else if (rx2[1] == "ERROR")
                                {
                                    session.Status = "Download failed: File not availible.";
                                    throw new Exception("File access error");
                                }
                                else
                                    throw new Exception("Invalid length response received by download client.");
                            }
                            else
                            {
                                throw new Exception("Invalid response received by download client.");
                            }
                        }
                        if (!proc)
                        {
                            Thread.Sleep(wait);
                            if (wait < 120)
                                wait += 10;
                            if (lastReceive + RECEIVE_TIMEOUT < Environment.TickCount)
                            {
                                throw new Exception("Receive timeout");
                            }
                        }
                    }
                    NetworkSpeedMeasurement nsm = new NetworkSpeedMeasurement();
                    if (rx2.Length == 3 && rx2[1] == "OK")
                    {
                        logService.AddInfo("Client download (" + req.FileName + "): Got OK");
                        session.Status = "Downloading: " + req.FileName + " in " + req.FolderPath;

                        string parentFolder = Path.GetDirectoryName(path);
                        if (!Directory.Exists(parentFolder))
                            Directory.CreateDirectory(parentFolder);

                        using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate))
                        {
                            long start = Environment.TickCount;
                            long fileLength = session.Length = long.Parse(rx2[2]);
                            long received = resumePoint;
                             
                            if(resumePoint!=0)
                              stream.Seek(resumePoint, SeekOrigin.Begin);


                            //Check for information already received
                            if (token.InputBufferLength > 0)
                            {
                                //Not sure if this is right??
                                foreach (var buffer in token.InputBufferBytes)
                                {
                                    stream.Write(buffer.Data, buffer.StartLocation, buffer.DataSize);
                                    received += buffer.DataSize;
                                }
                                token.ResetInputBuffer();
                            }

                            while (received < fileLength)
                            {
                                long dataLeft = (fileLength - received);
                                int rx = 0;
                                if (dataLeft < arg.Data.Length)
                                    rx = session.Socket.Receive(arg.Data, 0, (int)dataLeft, SocketFlags.None);
                                else
                                    rx = session.Socket.Receive(arg.Data, 0, arg.Data.Length, SocketFlags.None);
                                received += rx;
                                stream.Write(arg.Data, 0, rx);
                                session.Transfered = received;
                                if (session.Transfered > session.Length)
                                {

                                }
                                nsm.PutData(rx);
                                session.Speed = nsm.GetSpeed();
                            }
                            stream.Flush();
                            //  session.Status = "Download complete for " + req.FileName;
                            logService.AddInfo("Client download (" + req.FileName + ") complete in time :" + (Environment.TickCount - start) / 1000d);

                            connectionService.FreeClientSession(session);

                            //Thread.Sleep(40);
                        }
                    }
                    else
                    {

                    }
                }
                catch(Exception x)
                {
                    token.Dispose();
                    bufferService.FreeArg(arg);
                    throw x;
                }

                token.Dispose();
                bufferService.FreeArg(arg);
            }
        }

        public void FreeSession()
        {
            if (null != session)
                connectionService.FreeClientSession(session);
        }
    }
}
