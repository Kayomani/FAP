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
        private NetworkSpeedMeasurement netSpeed = new NetworkSpeedMeasurement();
        private Model model;
        private BufferService bufferService;

        private ReaderWriterLockSlim sync = new ReaderWriterLockSlim();
        private string status;
        private bool error;
        private bool run = true;
        private int outstandingRequests = 0;

        private int requestID = 1;

        public DownloadWorkerService(Model m, ConnectionService s, Node n, DownloadRequest r, BufferService b)
        {
            node = n;
            model = m;
            service = s;
            status = "Connecting..";
            bufferService =b;
            ThreadPool.QueueUserWorkItem(new WaitCallback(process));
        }

        public void Stop()
        {
            run = false;
        }

        public bool InError
        {
            get
            {
                sync.EnterReadLock();
                bool e = error;
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

        public int Percent
        {
            get { return 0; }
        }

        public int Size
        {
            get { return 0; }
        }

        public bool Completed
        {
            get
            {
                sync.EnterReadLock();
                bool count = queue.Count() == 0;
                sync.ExitReadLock();
                return count;
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

        public void AddDownload(DownloadRequest req)
        {
            sync.EnterWriteLock();
            if (!error)
            {
                queue.Add(new DownloadItem() { Request = req, UID = (requestID++).ToString() });
                req.State = DownloadRequestState.Requesting;
            }
            sync.ExitWriteLock();
        }

        public Node Node
        {
            get { return node; }
        }
        public int Downloads
        {
            get { return queue.Count; }
        }

        private void process(object o)
        {
            var session = service.GetClientSession(node);
            if (null == session)
            {
                OnError();
                return;
            }
            try
            {
                session.Socket.ReceiveBufferSize = BufferService.Buffer * 2;
                session.Socket.SendBufferSize = BufferService.SmallBuffer;
                DownloadVerb verb = new DownloadVerb();
                while (run)
                {
                    var arg = bufferService.GetArg();
                    
                    DownloadItem currentItem =  CheckForAndSendDownloadRequests(session);
                    ConnectionToken token = new ConnectionToken();

                    bool canDownload = false;

                    while (!canDownload)
                    {
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
                            OnError();
                            return;
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
                            if (count > 0)
                                status = "Downloading " + currentItem.Request.FileName + " [Requested " + count + "]";
                            else
                                status = "Downloading " + currentItem.Request.FileName;
                            canDownload = true;
                            sync.ExitWriteLock();

                            DownloadFile(currentItem, verb.ResumePoint, verb.FileSize,arg,session); 
                        }
                    }


                }
            }
            catch
            {
                service.FreeClientSession(session);
            }
        }

        private void OnError()
        {
            sync.EnterWriteLock();
            error = true;
            foreach (var w in queue)
                w.Request.State = DownloadRequestState.None;
            queue.Clear();
            sync.ExitWriteLock();
        }

        private void DownloadFile(DownloadItem i, long resumePoint, long length, MemoryBuffer arg, Session session)
        {
            if (resumePoint != 0)
                i.FileStream.Seek(resumePoint, SeekOrigin.Begin);
            long received = 0;

            while (received < length)
            {
                long dataLeft = length - received;
                int rx = 0;
                if (dataLeft < arg.Data.Length)
                    rx = session.Socket.Receive(arg.Data, 0, (int)dataLeft, SocketFlags.None);
                else
                    rx = session.Socket.Receive(arg.Data, 0, arg.Data.Length, SocketFlags.None);
                received += rx;
                i.FileStream.Write(arg.Data, 0, rx);

                sync.EnterWriteLock();
                netSpeed.PutData(rx);
                sync.ExitWriteLock();
            }
        }

        private DownloadItem CheckForAndSendDownloadRequests(Session s)
        {
            var item = queue.Pop();
            var req = item.Request;
            if (null != item)
            {
                //Open file to check file size
                FileStream stream;

                string mainPath = model.DownloadFolder + req.LocalPath + req.FolderPath;
                string incompletePath = model.IncompleteFolder + req.LocalPath + req.FolderPath;

                if (File.Exists(mainPath) || req.Size < BufferService.Buffer)
                    stream = File.Open(mainPath, FileMode.Append, FileAccess.Write, FileShare.Read);
                else
                    stream = File.Open(incompletePath, FileMode.Append, FileAccess.Write, FileShare.Read);

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
                    return item;
                }
                else
                {
                    stream.Close();
                    req.State = DownloadRequestState.Downloaded;
                   
                }
            }
            return null;
        }

        protected class DownloadItem
        {
            public DownloadRequest Request { set; get; }
            public string UID { set; get; }
            public FileStream FileStream { set; get; }
        }
    }
}
