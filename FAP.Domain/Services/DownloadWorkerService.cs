using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Domain.Entities;
using FAP.Domain.Verbs;
using FAP.Domain.Net;
using System.Net;
using System.IO;
using System.Reflection;
using Fap.Foundation;
using System.Threading;

namespace FAP.Domain.Services
{
    /// <summary>
    /// Handles processing a download queue 
    /// </summary>
    public class DownloadWorkerService : ITransferWorker
    {

        private BufferService bufferService;
        private object sync = new object();
        private Queue<DownloadRequest> queue = new Queue<DownloadRequest>();
        private NetworkSpeedMeasurement netSpeed = new NetworkSpeedMeasurement(NetSpeedType.Download);
        private Node remoteNode;
        private Model model;

        public DownloadWorkerService(Node n, Model m, BufferService b)
        {
            remoteNode = n;
            model = m;
            bufferService = b;
        }

        //ITransferWorker Members
        private long length;
        private bool isComplete = true;
        private string status;
        private long position;

        public Node Node
        {
            get { return remoteNode; }
        }

        public long Length
        {
            get { return length; }
        }

        public bool IsComplete
        {
            get { return isComplete; }
        }

        public long Speed
        {
            get { return netSpeed.GetSpeed(); }
        }

        public string Status
        {
            get { return status; }
        }

        public long Position
        {
            get { return position; }
        }

        public void AddDownload(DownloadRequest item)
        {
            lock (sync)
            {
                queue.Enqueue(item);
                if (isComplete)
                {
                    isComplete = false;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(process));
                }
                item.State = DownloadRequestState.Requesting;
               
            }
        }

        private void process(object o)
        {
            try
            {
                while (true)
                {
                    DownloadRequest currentItem = null;
                    lock (sync)
                    {
                        if (queue.Count > 0)
                            currentItem = queue.Dequeue();
                        if (null == currentItem)
                        {
                            isComplete = true;
                            return;
                        }
                    }

                    if (currentItem.IsFolder)
                    {
                        length =0;
                        position =0;
                        status = "Downloading folder info for " + currentItem.FullPath;
                        //Item is a folder - Just get the folder items and add them to the queue.
                        BrowseVerb verb = new BrowseVerb(null, null);
                        verb.Path = currentItem.FullPath;
                        //Always get the latest info.
                        verb.NoCache = true;

                        Client client = new Client(null);

                        if (client.Execute(verb, remoteNode))
                        {
                            List<DownloadRequest> newItems = new List<DownloadRequest>();

                            foreach (var item in verb.Results)
                            {
                                newItems.Add(new DownloadRequest()
                                               {
                                                   Added = DateTime.Now,
                                                   ClientID = remoteNode.ID,
                                                   FullPath = currentItem.FullPath + "/" + item.Name,
                                                   IsFolder = item.IsFolder,
                                                   LocalPath = currentItem.LocalPath + "/" + currentItem.FileName,
                                                   NextTryTime = 0,
                                                   Nickname = remoteNode.Nickname,
                                                   Size = item.Size,
                                                   State = DownloadRequestState.None
                                               });
                            }
                            model.DownloadQueue.List.AddRange(newItems);
                        }
                        else
                        {
                            currentItem.State = DownloadRequestState.Error;
                            currentItem.NextTryTime = Environment.TickCount + Model.DOWNLOAD_RETRY_TIME;
                        }
                    }
                    else
                    {
                        MemoryBuffer buffer = bufferService.GetBuffer();
                        buffer.SetDataLocation(0, buffer.Data.Length);
                        //Item is a file - download it
                        try
                        {
                            length = currentItem.Size;
                            status = currentItem.Nickname + " - " + currentItem.FileName + " - " + Utility.FormatBytes(currentItem.Size);
                            currentItem.State = DownloadRequestState.Downloading;

                            FileStream fileStream = null;

                            StringBuilder mainsb = new StringBuilder();
                           // mainsb.Append(model.DownloadFolder);
                           // 
                            mainsb.Append(currentItem.LocalPath);
                            mainsb.Append("\\");
                             string mainfolder = mainsb.ToString();

                             mainsb.Append("\\");
                            mainsb.Append(currentItem.FileName);

                            StringBuilder incompletesb = new StringBuilder();
                           // incompletesb.Append(model.IncompleteFolder);
                           
                            incompletesb.Append(currentItem.LocalPath);
                            incompletesb.Append("\\");
                            string incompleteFolder = incompletesb.ToString();

                            incompletesb.Append("\\");
                            incompletesb.Append(currentItem.FileName);

                            string mainPath = mainsb.ToString();
                           
                            string incompletePath = incompletesb.ToString();


                            //Check to see if the file already exists.
                            if (File.Exists(mainPath))
                            {
                                //File exists in the download directory.
                                fileStream = File.Open(mainPath, FileMode.Open, FileAccess.Write, FileShare.None);
                            }
                            else
                            {
                                if(!Directory.Exists(incompleteFolder))
                                    Directory.CreateDirectory(incompleteFolder);

                                //Else resume or just create
                                fileStream = File.Open(incompletePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                            }


                            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(getDownloadUrl(currentItem.FullPath));
                            
                            

                            //If we are resuming then add range
                            if (fileStream.Length != 0)
                            {
                                //Yes Micrsoft if you read this...  OH WHY IS ADDRANGE ONLY AN INT?? We live in an age where we might actually download more than 2gb
                                //req.AddRange(fileStream.Length);

                                //Hack
                                MethodInfo method = typeof(WebHeaderCollection).GetMethod("AddWithoutValidate", BindingFlags.Instance | BindingFlags.NonPublic);
                                string key = "Range";
                                string val = string.Format("bytes={0}", fileStream.Length);
                                method.Invoke(req.Headers, new object[] { key, val });
                                position =fileStream.Length;
                            }

                            System.Net.HttpWebResponse resp = (System.Net.HttpWebResponse)req.GetResponse();

                            
                            if (resp.ContentLength > 0)
                            {
                                using (Stream responseStream = resp.GetResponseStream())
                                {
                                    long readLength = 0;

                                    while(readLength<resp.ContentLength)
                                    {
                                        int read = responseStream.Read(buffer.Data, 0, buffer.DataSize);
                                        if (read == 0)
                                        {
                                           break;
                                        }
                                        else
                                        {
                                            fileStream.Write(buffer.Data, 0, read);
                                            readLength += read;
                                            position += read;
                                            netSpeed.PutData(read);
                                        }
                                    }
                                }
                            }
                            model.DownloadQueue.List.Remove(currentItem);
                            currentItem.State = DownloadRequestState.Downloaded;
                            fileStream.Close();
                            fileStream.Dispose();
                            status = currentItem.Nickname + " - Complete: " + currentItem.FileName;
                            resp.Close();

                        }
                        catch
                        {
                            currentItem.State = DownloadRequestState.Error;
                            currentItem.NextTryTime = Environment.TickCount + Model.DOWNLOAD_RETRY_TIME;
                        }
                        finally
                        {
                            bufferService.FreeBuffer(buffer);
                        }
                    }
                }
            }
            catch
            {
                //Something went very wrong.  Clear the queue and die.
                lock (sync)
                {
                    isComplete = true;
                    foreach (var v in queue)
                        v.State = DownloadRequestState.None;
                    queue.Clear();
                }
            }
        }



        private string getDownloadUrl(string path)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("http://");
            sb.Append(remoteNode.Location);
            if (!remoteNode.Location.EndsWith("/"))
                sb.Append("/");
            sb.Append(path);
            return sb.ToString();
        }
    }
}
