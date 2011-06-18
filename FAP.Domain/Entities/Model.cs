#region Copyright Kayomani 2011.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.

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
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Xml.Serialization;
using FAP.Domain.Entities.FileSystem;
using FAP.Domain.Net;
using FAP.Domain.Verbs;
using Fap.Foundation;
using Fap.Foundation.Services;
using Newtonsoft.Json;
using NLog;
using Directory = System.IO.Directory;
using File = System.IO.File;
using System.ComponentModel;

namespace FAP.Domain.Entities
{
    [Serializable]
    public class Model : BaseEntity, IDataErrorInfo
    {
        public static readonly string AppVersion = "FAP Beta 2";
        public static readonly string ProtocolVersion = "FAP/1.0";
        public static int UPLINK_TIMEOUT = 60000; //1 minute
        public static int DOWNLOAD_RETRY_TIME = 120000; //2minutes
        public static int FREE_FILE_LIMIT = 1048576; //1mb
        public static int MAX_SEARCH_RESULTS = 10000;
        private readonly SafeObservedCollection<TransferLog> downloads = new SafeObservedCollection<TransferLog>();

        private readonly string saveLocation = "ClientConfig.cfg";
        private readonly ReaderWriterLockSlim shutdownLock;

        private readonly SafeObservedCollection<TransferSession> transferSessions =
            new SafeObservedCollection<TransferSession>();

        private readonly SafeObservingCollection<TransferLog> uiDownloads;

        private readonly SafeObservingCollection<TransferSession> uiTransferSession;
        private readonly SafeObservingCollection<TransferLog> uiUploads;
        private readonly SafeObservedCollection<TransferLog> uploads = new SafeObservedCollection<TransferLog>();
        private bool alwaysNoCacheBrowsing;
        private bool disableCompare;
        private bool displayedHelp;
        private string downloadFolder;
        private DownloadQueue downloadQueue;
        private string incompleteFolder;
        private int maxDownloads;
        private int maxDownloadsPerUser;
        private int maxUploads;
        private int maxUploadsPerUser;
        private SafeObservedCollection<string> messages = new SafeObservedCollection<string>();
        private Network network = new Network();
        private Node node;
        private OverlordPriority overlordPriority;
        private SafeObservedCollection<Share> shares = new SafeObservedCollection<Share>();

        public Model()
        {
            node = new Node();
            downloadQueue = new DownloadQueue();
            shutdownLock = new ReaderWriterLockSlim();
            uiTransferSession = new SafeObservingCollection<TransferSession>(transferSessions);
            uiDownloads = new SafeObservingCollection<TransferLog>(downloads);
            uiUploads = new SafeObservingCollection<TransferLog>(uploads);
        }

        [JsonIgnore]
        public SafeObservedCollection<TransferLog> CompletedDownloads
        {
            get { return downloads; }
        }

        [JsonIgnore]
        public SafeObservingCollection<TransferLog> UICompletedDownloads
        {
            get { return uiDownloads; }
        }

        [JsonIgnore]
        public SafeObservedCollection<TransferLog> CompletedUploads
        {
            get { return uploads; }
        }

        [JsonIgnore]
        public SafeObservingCollection<TransferLog> UICompletedUploads
        {
            get { return uiUploads; }
        }

        [JsonIgnore]
        public SafeObservedCollection<string> Messages
        {
            get { return messages; }
            set
            {
                messages = value;
                NotifyChange("Messages");
            }
        }

        public string LocalSecret
        {
            set { node.Secret = value; }
            get { return node.Secret; }
        }

        public string Nickname
        {
            set { node.Nickname = value; }
            get { return node.Nickname; }
        }

        public string Description
        {
            set { node.Description = value; }
            get { return node.Description; }
        }

        public bool DisplayedHelp
        {
            set { displayedHelp = value; }
            get { return displayedHelp; }
        }

        public string Avatar
        {
            set
            {
                node.Avatar = value;
                NotifyChange("Avatar");
            }
            get { return node.Avatar; }
        }

        [XmlIgnore]
        [JsonIgnore]
        public DownloadQueue DownloadQueue
        {
            get { return downloadQueue; }
            set { downloadQueue = value; }
        }

        public Node LocalNode
        {
            set
            {
                node = value;
                NotifyChange("LocalNode");
            }
            get { return node; }
        }

        public OverlordPriority OverlordPriority
        {
            set { overlordPriority = value; }
            get { return overlordPriority; }
        }

        [JsonIgnore]
        public bool IsDedicated { set; get; }

        [JsonIgnore]
        public Network Network
        {
            set
            {
                network = value;
                NotifyChange("Network");
            }
            get { return network; }
        }

        public SafeObservedCollection<Share> Shares
        {
            set
            {
                shares = value;
                NotifyChange("Shares");
            }
            get { return shares; }
        }


        public bool AlwaysNoCacheBrowsing
        {
            set
            {
                alwaysNoCacheBrowsing = value;
                NotifyChange("AlwaysNoCacheBrowsing");
            }
            get { return alwaysNoCacheBrowsing; }
        }

        [XmlIgnore]
        [JsonIgnore]
        public int ClientPort { set; get; }

        public string DownloadFolder
        {
            set
            {
                downloadFolder = value;
                NotifyChange("DownloadFolder");
            }
            get { return downloadFolder; }
        }

        public string IncompleteFolder
        {
            set
            {
                incompleteFolder = value;
                NotifyChange("IncompleteFolder");
            }
            get { return incompleteFolder; }
        }

        public bool DisableComparision
        {
            set { disableCompare = value; }
            get { return disableCompare; }
        }

        [XmlIgnore]
        [JsonIgnore]
        public SafeObservedCollection<TransferSession> TransferSessions
        {
            get { return transferSessions; }
        }


        [XmlIgnore]
        [JsonIgnore]
        public SafeObservingCollection<TransferSession> UITransferSessions
        {
            get { return uiTransferSession; }
        }

        public int MaxDownloads
        {
            set
            {
                maxDownloads = value;
                NotifyChange("MaxDownloads");
            }
            get { return maxDownloads; }
        }

        public int MaxDownloadsPerUser
        {
            set
            {
                maxDownloadsPerUser = value;
                NotifyChange("MaxDownloadsPerUser");
            }
            get { return maxDownloadsPerUser; }
        }

        public int MaxUploads
        {
            set
            {
                maxUploads = value;
                NotifyChange("MaxUploads");
            }
            get { return maxUploads; }
        }

        public int MaxUploadsPerUser
        {
            set
            {
                maxUploadsPerUser = value;
                NotifyChange("maxUploadsPerUser");
            }
            get { return maxUploadsPerUser; }
        }

        /// <summary>
        /// Called by methods which should not be interupted such as saving the download list.
        /// </summary>
        public void GetAntiShutdownLock()
        {
            shutdownLock.EnterReadLock();
        }

        /// <summary>
        /// Must be called after GetAntiShutdownLock to allow for shutdown to occur
        /// </summary>
        public void ReleaseAntiShutdownLock()
        {
            shutdownLock.ExitReadLock();
        }

        /// <summary>
        /// Called by the core upon shutdown to stop further saves.
        /// </summary>
        public void GetShutdownLock()
        {
            shutdownLock.TryEnterWriteLock(4000);
        }

        public void Save()
        {
            lock (downloadQueue)
            {
                SafeSave(this, saveLocation, Formatting.Indented);
            }
        }

        public void Load()
        {
            lock (downloadQueue)
            {
                try
                {
                    if (File.Exists(DATA_FOLDER + saveLocation))
                    {
                        var saved = SafeLoad<Model>(saveLocation);

                        Shares.Clear();
                        Shares.AddRange(saved.Shares.OrderBy(s => s.Name).ToList());
                        Avatar = saved.Avatar;
                        Description = saved.Description;
                        Nickname = saved.Nickname;
                        DownloadFolder = saved.DownloadFolder;
                        MaxDownloads = saved.MaxDownloads;
                        MaxDownloadsPerUser = saved.MaxDownloadsPerUser;
                        MaxUploads = saved.MaxUploads;
                        MaxUploadsPerUser = saved.MaxUploadsPerUser;
                        DisableComparision = saved.DisableComparision;
                        LocalNode = saved.LocalNode;
                        AlwaysNoCacheBrowsing = saved.AlwaysNoCacheBrowsing;
                        OverlordPriority = saved.OverlordPriority;
                        DisplayedHelp = saved.DisplayedHelp;
                    }
                    else if (File.Exists(Legacy.Model.saveLocation))
                    {
                        //New config doesnt exist but an older version does, try to import.
                        var oldmodel = new Legacy.Model();
                        oldmodel.Load();

                        Shares.Clear();
                        Shares.AddRange(oldmodel.Shares.OrderBy(s => s.Name).ToList());
                        Avatar = oldmodel.Avatar;
                        Description = oldmodel.Description;
                        Nickname = oldmodel.Nickname;
                        DownloadFolder = oldmodel.DownloadFolder;
                        MaxDownloads = oldmodel.MaxDownloads;
                        MaxDownloadsPerUser = oldmodel.MaxDownloadsPerUser;
                        MaxUploads = oldmodel.MaxUploads;
                        MaxUploadsPerUser = oldmodel.MaxUploadsPerUser;
                        DisableComparision = oldmodel.DisableComparision;
                        LocalNode = new Node();
                        foreach (var data in oldmodel.Node.Data)
                            LocalNode.SetData(data.Key, data.Value);
                        AlwaysNoCacheBrowsing = oldmodel.AlwaysNoCacheBrowsing;
                        OverlordPriority = OverlordPriority.Normal;
                        Save();
                    }
                }
                catch (Exception e)
                {
                    LogManager.GetLogger("faplog").WarnException("Failed to read config", e);
                }
            }
        }

        public void CheckSetDefaults()
        {
            if (string.IsNullOrEmpty(LocalNode.ID))
                LocalNode.ID = IDService.CreateID();

            if (string.IsNullOrEmpty(LocalNode.Secret))
                LocalNode.Secret = IDService.CreateID();

            if (LocalNode.Port == 0)
                LocalNode.Port = 30;

            //If there is no avatar set then set the default
            if (string.IsNullOrEmpty(Avatar))
            {
                Stream stream =
                    Application.GetResourceStream(new Uri("Images/Default_Avatar.png", UriKind.Relative)).Stream;
                var img = new byte[stream.Length];
                stream.Read(img, 0, (int) stream.Length);
                Avatar = Convert.ToBase64String(img);
                Save();
            }
            //Set default nick
            if (string.IsNullOrEmpty(Nickname))
            {
                //Try to use the username
                string user = WindowsIdentity.GetCurrent().Name;
                if (!string.IsNullOrEmpty(user) && user.Contains('\\'))
                {
                    user = user.Substring(user.IndexOf('\\') + 1);
                }
                //If the username is a default one then use the PC name
                if (string.IsNullOrEmpty(user) ||
                    string.Equals(user, "Administrator", StringComparison.InvariantCultureIgnoreCase) ||
                    string.Equals(user, "Guest", StringComparison.InvariantCultureIgnoreCase))
                    user = Dns.GetHostName();
                Nickname = user;
            }

            //Set default limits
            if (MaxDownloads == 0)
                MaxDownloads = 3;
            if (MaxDownloadsPerUser == 0)
                MaxDownloadsPerUser = 3;
            if (MaxUploads == 0)
                MaxUploads = 3;
            if (MaxUploadsPerUser == 0)
                MaxUploadsPerUser = 4;

            //Set default download folder
            if (string.IsNullOrEmpty(DownloadFolder))
                DownloadFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) +
                                 "\\FAP Downloads";

            if (!Directory.Exists(DownloadFolder))
                Directory.CreateDirectory(DownloadFolder);

            //Set incomplete download folder
            if (string.IsNullOrEmpty(IncompleteFolder) || !Directory.Exists(IncompleteFolder))
                IncompleteFolder = DownloadFolder + "\\Incomplete";

            if (!Directory.Exists(DownloadFolder))
                Directory.CreateDirectory(DownloadFolder);

            if (!Directory.Exists(IncompleteFolder))
                Directory.CreateDirectory(IncompleteFolder);
        }

        /// <summary>
        /// Create a new download queue item based on a url.  Takes URLs in the format fap://NodeID/path/to/file
        /// </summary>
        /// <param name="url"></param>
        public void AddDownloadURL(string url)
        {
            string parentDir = Utility.DecodeURL(url);
            //Strip protocol
            if (parentDir.StartsWith("fap://", StringComparison.InvariantCultureIgnoreCase))
                parentDir = parentDir.Substring(6);


            int index = parentDir.LastIndexOf('/');
            if (index == -1)
            {
                LogManager.GetLogger("faplog").Error("Unable to add download as an invalid url as passed!");
            }
            else
            {
                string fileName = parentDir.Substring(index + 1);
                parentDir = parentDir.Substring(0, index);

                string nodeId = parentDir.Substring(0, parentDir.IndexOf('/'));
                parentDir = parentDir.Substring(nodeId.Length + 1);
                Node node = network.Nodes.Where(n => n.ID == nodeId).FirstOrDefault();

                if (null == node)
                {
                    //Node not found
                    LogManager.GetLogger("faplog").Error("Unable to add download as node {0} was not found!", nodeId);
                }
                else
                {
                    //Node found - browse to get info
                    var verb = new BrowseVerb(null, null);
                    verb.NoCache = false;
                    verb.Path = parentDir;

                    var client = new Client(LocalNode);

                    if (client.Execute(verb, node))
                    {
                        BrowsingFile remoteFile = verb.Results.Where(f => f.Name == fileName).FirstOrDefault();
                        if (null != remoteFile)
                        {
                            downloadQueue.List.Add(new DownloadRequest
                                                       {
                                                           Added = DateTime.Now,
                                                           FullPath = parentDir + "/" + remoteFile.Name,
                                                           IsFolder = remoteFile.IsFolder,
                                                           Size = remoteFile.Size,
                                                           LocalPath = null,
                                                           State = DownloadRequestState.None,
                                                           ClientID = node.ID,
                                                           Nickname = node.Nickname
                                                       });
                        }
                        else
                        {
                            LogManager.GetLogger("faplog").Error(
                                "Unable to add download as {0} was not found on the remote server!", fileName);
                        }
                    }
                    else
                    {
                        LogManager.GetLogger("faplog").Error("Unable to add download as node {0} was not accessible!",
                                                             nodeId);
                    }
                }
            }
        }

        public string Error
        {
            get { return this[null]; }
        }

        public string this[string columnName]
        {
            get
            {
                if(null==columnName || columnName == "Nickname")
                {
                    if (string.IsNullOrEmpty(Nickname))
                        return "Please enter a nickname";
                }
                if (null == columnName || columnName == "MaxDownloads")
                {
                    if (MaxDownloads < 0)
                        return "Please enter a positive number";
                }
                if (null == columnName || columnName == "MaxDownloadsPerUser")
                {
                    if (MaxDownloadsPerUser < 0)
                        return "Please enter a positive number";
                }
                if (null == columnName || columnName == "MaxUploads")
                {
                    if (MaxUploads < 1)
                        return "You must allow atleast one upload!";
                }
                return null;
            }
        }
    }
}