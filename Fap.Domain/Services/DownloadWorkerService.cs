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
using Fap.Domain.Entity;
using Fap.Foundation;
using Fap.Network.Services;
using System.Threading;
using System.IO;
using Fap.Domain.Verbs;
using Fap.Network;
using System.Net.Sockets;
using Fap.Foundation.Logging;

namespace Fap.Domain.Services
{
    public class DownloadWorkerService : ITransferWorker
    {
        private Node node;
        private BackgroundSafeObservable<DownloadItem> queue = new BackgroundSafeObservable<DownloadItem>();
        private ConnectionService service;
        private NetworkSpeedMeasurement netSpeed = new NetworkSpeedMeasurement(NetSpeedType.Download);
        private Model model;
        private BufferService bufferService;
        private ReaderWriterLockSlim sync = new ReaderWriterLockSlim();
        private Logger logger;

        private bool complete;
        private string status;
        private long length = 0;
        private long received;

        private int lastRequestID = 1;
        private int oustandingRequests = 0;

        public delegate void DownloaderFinished();
        public event DownloaderFinished OnDownloaderFinished;

        public DownloadWorkerService(Model m, ConnectionService s, Node n, DownloadRequest r, BufferService b, DownloadRequest req, Logger l)
        {
            logger = l;
            node = n;
            model = m;
            service = s;
            status = "Connecting..";
            bufferService =b;
            AddDownload(req);
            ThreadPool.QueueUserWorkItem(new WaitCallback(process));
        }

        public void Stop()
        {
            complete = true;
        }

        #region Properties
        public Node Node
        {
            get { return node; }
        }

        public int Downloads
        {
            get { return queue.Count; }
        }

        public bool IsComplete
        {
            get
            {
                return complete;
            }
        }

        public string Status
        {
            get
            {
                return status;
            }
        }

        public long Speed
        {
            get 
            {
                return netSpeed.GetSpeed();
            }
        }

        public long Position
        {
            get{
                return received;
            }
        }

        public long Length
        {
            get
            {
                return length;
            }
        }

        public bool IsBusy
        {
            get
            {
                var list = queue.ToList();
                if (queue.Count > 5)
                    return true;
                return list.Select(i => i.Request.Size).Sum() > 22428800;//20mb
            }
        }
        #endregion
       
        public void AddDownload(DownloadRequest req)
        {
            sync.EnterWriteLock();

            if (req.State != DownloadRequestState.None)
            {
            }

            if (!complete)
            {
                queue.Add(new DownloadItem() { Request = req, UID = (lastRequestID++).ToString() });
                req.State = DownloadRequestState.Requesting;
            }
            sync.ExitWriteLock();
        }

        private void OnError()
        {
            sync.EnterWriteLock();
            complete = true;
            foreach (var w in queue)
            {
                w.Request.NextTryTime = Environment.TickCount + 60000;
                w.Request.State = DownloadRequestState.None;
            }
            queue.Clear();
            sync.ExitWriteLock();
        }

        private void process(object o)
        {
            var session = service.GetClientSession(node);
            if (null == session)
            {
                OnError();
                return;
            }
            var arg = bufferService.GetArg();
            try
            {
                session.Socket.Blocking = true;
                session.Socket.ReceiveBufferSize = BufferService.Buffer * 2;
                session.Socket.SendBufferSize = BufferService.SmallBuffer;
                DownloadVerb verb = new DownloadVerb();
                while (!complete)
                {
                    CheckForAndSendDownloadRequests(session);
                    if (oustandingRequests > 0)
                    {
                        ConnectionToken token = new ConnectionToken();
                        var currentItem = queue.First();
                        length = currentItem.Request.Size;
                        do
                        {
                            //Receive header
                            int rx = session.Socket.Receive(arg.Data);
                            arg.SetDataLocation(0, rx);
                            token.ReceiveData(arg);
                        }
                        while (!token.ContainsCommand());

                        Response response = new Response();
                        Mediator.Deserialize(token.GetCommand(), out response);
                        verb.ReceiveResponse(response);

                        if (currentItem.UID != response.RequestID)
                        {
                            //Wrong file id.. erk what did they send us??
                            throw new Exception();
                        }

                        if (currentItem.Request.IsFolder)
                        {
                            BrowseVerb cmd = new BrowseVerb(model);
                            cmd.ReceiveResponse(response);
                            oustandingRequests--;
                            if (cmd.Status != 0)
                            {
                                sync.EnterWriteLock();
                                status = "Download complete: " + currentItem.Request.FileName;
                                currentItem.Request.State = DownloadRequestState.Downloaded;
                                queue.Remove(currentItem);
                                sync.ExitWriteLock();
                            }
                            else
                            {
                                sync.EnterWriteLock();
                                status = "Download complete: " + currentItem.Request.FileName;
                                currentItem.Request.State = DownloadRequestState.Downloaded;
                                queue.Remove(currentItem);
                                sync.ExitWriteLock();

                                foreach (var result in cmd.Results)
                                {
                                    DownloadRequest request = new DownloadRequest();
                                    request.Added = DateTime.Now;
                                    request.FullPath = currentItem.Request.FullPath + "\\" + result.Name;
                                    request.IsFolder = result.IsFolder;
                                    request.Size = result.Size;
                                    request.State = DownloadRequestState.None;
                                    request.ClientID = currentItem.Request.ClientID;
                                    request.Nickname = currentItem.Request.Nickname;
                                    request.LocalPath = currentItem.Request.LocalPath + "\\" + currentItem.Request.FileName + "\\";

                                    if (!IsBusy)
                                        AddDownload(request);
                                    else
                                    {
                                        model.DownloadQueue.List.Insert(0, request);
                                    }
                                }
                            }
                        }
                        else if (verb.Error)
                        {
                            sync.EnterWriteLock();
                            queue.Remove(currentItem);
                            currentItem.Request.NextTryTime = Environment.TickCount + 60000;
                            currentItem.Request.State = DownloadRequestState.None;
                            status = verb.ErrorMsg + " " + currentItem.Request.FileName;
                            sync.ExitWriteLock();
                            oustandingRequests--;
                        }
                        else if (verb.InQueue)
                        {
                            sync.EnterWriteLock();
                            status = "Queue position " + verb.QueuePosition + " for " + currentItem.Request.FileName;
                            sync.ExitWriteLock();
                        }
                        else
                        {
                            sync.EnterWriteLock();
                            int count = queue.Count;
                            if (count > 1)
                                status = "Downloading " + currentItem.Request.FileName + " [Requested " + count + "]";
                            else
                                status = "Downloading " + currentItem.Request.FileName;
                            sync.ExitWriteLock();
                            try
                            {
                                DownloadFile(currentItem, verb.ResumePoint, verb.FileSize, arg, session,token);
                            }
                            finally
                            {
                                currentItem.FileStream.Flush();
                                currentItem.FileStream.Close();
                                if (currentItem.FileStream.Name.Contains(model.IncompleteFolder))
                                {
                                    string destination = currentItem.FileStream.Name.Replace(model.IncompleteFolder, model.DownloadFolder);
                                    string destinationDir = Path.GetDirectoryName(destination);
                                    if (!Directory.Exists(destinationDir))
                                        Directory.CreateDirectory(destinationDir);

                                    File.Move(currentItem.FileStream.Name, destination);
                                }
                            }
                            sync.EnterWriteLock();
                            status = "Download complete: " + currentItem.Request.FileName;
                            currentItem.Request.State = DownloadRequestState.Downloaded;
                            queue.Remove(currentItem);
                            sync.ExitWriteLock();
                        }
                    }
                    if (!complete)
                    {
                        long startTime = Environment.TickCount;
                        //Pause completing the download for 1 second as we 

                        if (queue.Count == 0 && null != OnDownloaderFinished)
                            OnDownloaderFinished();

                        while (queue.Count == 0)
                        {
                            Thread.Sleep(15);

                            sync.EnterWriteLock();
                            if (queue.Count == 0 && Environment.TickCount - startTime > 1000)
                            {
                                complete = true;
                                break;

                            }
                            sync.ExitWriteLock();
                        }
                    }
                }
            }
            catch
            {
                OnError();
                session.Socket.Close();
                service.RemoveClientSession(session);
            }
            finally
            {
                bufferService.FreeArg(arg);
                service.FreeClientSession(session);
                logger.AddInfo("Downloader quit");
            }
        }

        private void DownloadFile(DownloadItem i, long resumePoint, long length, MemoryBuffer arg, Session session, ConnectionToken token)
        {
            if (resumePoint != 0)
                i.FileStream.Seek(resumePoint, SeekOrigin.Begin);
            received = resumePoint;

            //Check for data that may be in the same buffer as the response data
            if (token.InputBufferLength > 0)
            {
                foreach (var buffer in token.RawInputBuffers)
                {
                    i.FileStream.Write(buffer.Data, buffer.StartLocation, buffer.DataSize);
                    received += buffer.DataSize;
                    Console.WriteLine("pre buf for " + i.FileStream.Name);
                }
                token.ResetInputBuffer();
            }

            while (received < length)
            {
                long dataLeft = length - received;
                int rx = 0;
                if (dataLeft < arg.Data.Length)
                    rx = session.Socket.Receive(arg.Data, 0, (int)dataLeft, SocketFlags.None);
                else
                    rx = session.Socket.Receive(arg.Data, 0, arg.Data.Length, SocketFlags.None);
                i.FileStream.Write(arg.Data, 0, rx);

                sync.EnterWriteLock();
                received += rx;
                netSpeed.PutData(rx);
                sync.ExitWriteLock();
            }
            oustandingRequests--;
        }

        private void CheckForAndSendDownloadRequests(Session s)
        {
            var item = queue.FirstOrDefault();
            if (null != item)
            {
                if (item.Requested)
                    return;

                item.Requested = true;
                if (item.Request.IsFolder)
                {
                    BrowseVerb cmd = new BrowseVerb(model);
                    cmd.Path = item.Request.FullPath;
                    cmd.RequestID = item.UID;
                    s.Socket.Send(Mediator.Serialize(cmd.CreateRequest()));
                    s.Socket.Blocking = true;
                    oustandingRequests++;
                }
                else
                {
                    FileStream stream = null;
                    var req = item.Request;
                    try
                    {
                        
                        //Open file to check file size
                       

                        StringBuilder main = new StringBuilder();
                        main.Append(model.DownloadFolder);
                        main.Append("\\");
                        main.Append(req.LocalPath);
                        main.Append(req.FileName);

                        StringBuilder incomplete = new StringBuilder();
                        incomplete.Append(model.IncompleteFolder);
                        incomplete.Append("\\");
                        incomplete.Append(req.LocalPath);

                        string mainPath = main.ToString();
                        string incompleteFolder = incomplete.ToString();

                        incomplete.Append("\\");
                        incomplete.Append(req.FileName);

                        string incompletePath = incomplete.ToString();

                        if (File.Exists(mainPath))
                            stream = File.Open(mainPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                        else
                        {
                            if (!Directory.Exists(incompleteFolder))
                                Directory.CreateDirectory(incompleteFolder);

                            stream = File.Open(incompletePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                        }

                        //If file size less than expected size then download
                        if (stream.Length < req.Size)
                        {
                            DownloadVerb verb = new DownloadVerb();
                            verb.Path = req.FullPath;
                            verb.ID = item.UID;
                            verb.ResumePoint = stream.Length;
                            item.FileStream = stream;
                            s.Socket.Send(Mediator.Serialize(verb.CreateRequest()));
                            s.Socket.Blocking = true;
                            oustandingRequests++;
                        }
                        else
                        {
                            stream.Close();
                            req.State = DownloadRequestState.Downloaded;
                            queue.Remove(item);
                        }
                    }
                    catch
                    {
                        if(null!=stream && stream.CanWrite)
                        stream.Close();
                        req.NextTryTime = Environment.TickCount+60000;
                        req.State = DownloadRequestState.None;
                        queue.Remove(item);
                    }
                }
            }
        }

        protected class DownloadItem
        {
            public DownloadRequest Request { set; get; }
            public string UID { set; get; }
            public FileStream FileStream { set; get; }
            public bool Requested { set; get; }
        }
    }
}
