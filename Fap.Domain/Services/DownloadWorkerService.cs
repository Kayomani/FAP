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

namespace Fap.Domain.Services
{
    public class DownloadWorkerService
    {
        private Node node;
        private BackgroundSafeObservable<DownloadItem> queue = new BackgroundSafeObservable<DownloadItem>();
        private ConnectionService service;
        private NetworkSpeedMeasurement netSpeed = new NetworkSpeedMeasurement(NetSpeedType.Download);
        private Model model;
        private BufferService bufferService;
        private ReaderWriterLockSlim sync = new ReaderWriterLockSlim();

        private bool complete;
        private int lastRequestID = 1;

        private string status;
        private long length = 0;
        private long received;

        public DownloadWorkerService(Model m, ConnectionService s, Node n, DownloadRequest r, BufferService b, DownloadRequest req)
        {
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
            sync.EnterWriteLock();
            complete = true;
            sync.ExitWriteLock();
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
                sync.EnterReadLock();
                bool e = complete;
                sync.ExitReadLock();
                return e;
            }
        }

        public string Status
        {
            get
            {
                sync.EnterReadLock();
                string status = this.status;
                sync.ExitReadLock();
                return status;
            }
        }

        public long Speed
        {
            get 
            {
                sync.EnterReadLock();
                long speed = netSpeed.GetSpeed();
                sync.ExitReadLock();
                return speed;
            }
        }

        public long Received
        {
            get 
            {
                sync.EnterReadLock();
                long v = received;
                sync.ExitReadLock();
                return v;
            }
        }

        public long Length
        {
            get
            {
                sync.EnterReadLock();
                long v = length;
                sync.ExitReadLock();
                return v;
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
                session.Socket.ReceiveBufferSize = BufferService.Buffer * 2;
                session.Socket.SendBufferSize = BufferService.SmallBuffer;
                DownloadVerb verb = new DownloadVerb();
                while (!complete)
                {

                    CheckForAndSendDownloadRequests(session);
                    ConnectionToken token = new ConnectionToken();

                    var currentItem = queue.First();
                    length = currentItem.Request.Size;
                    do
                    {
                        //Receive header
                        arg.DataSize = session.Socket.Receive(arg.Data);
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

                    if (verb.Error)
                    {
                        sync.EnterWriteLock();
                        queue.RemoveAt(0);
                        currentItem.Request.NextTryTime = Environment.TickCount + 60000;
                        currentItem.Request.State = DownloadRequestState.None;
                        status = verb.ErrorMsg + " " + currentItem.Request.FileName;
                        sync.ExitWriteLock();
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
                            DownloadFile(currentItem, verb.ResumePoint, verb.FileSize, arg, session);
                            
                        }
                        finally
                        {
                            currentItem.FileStream.Flush();
                            currentItem.FileStream.Close();
                            if (currentItem.FileStream.Name.Contains(model.IncompleteFolder))
                            {
                                File.Move(currentItem.FileStream.Name,
                                    currentItem.FileStream.Name.Replace(model.IncompleteFolder, model.DownloadFolder));
                            }
                        }
                    }
                    status = "Download complete: " + currentItem.Request.FileName;
                    currentItem.Request.State = DownloadRequestState.Downloaded;
                    queue.Remove(currentItem);

                     sync.EnterWriteLock();
                     if (!complete)
                     {
                         if (queue.Count == 0)
                             complete = true;
                     }
                     sync.ExitWriteLock();
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
            }
        }

        private void DownloadFile(DownloadItem i, long resumePoint, long length, MemoryBuffer arg, Session session)
        {
            if (resumePoint != 0)
                i.FileStream.Seek(resumePoint, SeekOrigin.Begin);
            received = resumePoint;

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
        }

        private void CheckForAndSendDownloadRequests(Session s)
        {
            var item = queue.FirstOrDefault();
            if (null != item)
            {
                var req = item.Request;
                //Open file to check file size
                FileStream stream;

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

                if (File.Exists(mainPath) || req.Size < BufferService.Buffer)
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
                    SocketAsyncEventArgs  arg = new SocketAsyncEventArgs();
                    byte[] data = Mediator.Serialize(verb.CreateRequest());
                    arg.SetBuffer(data,0,data.Length);
                    s.Socket.SendAsync(arg);
                }
                else
                {
                    stream.Close();
                    req.State = DownloadRequestState.Downloaded;
                    model.DownloadQueue.List.Remove(req);
                }
            }
        }

        protected class DownloadItem
        {
            public DownloadRequest Request { set; get; }
            public string UID { set; get; }
            public FileStream FileStream { set; get; }
        }
    }
}
