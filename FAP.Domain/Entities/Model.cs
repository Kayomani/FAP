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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using Fap.Foundation;
using NLog;
using Newtonsoft.Json;

namespace FAP.Domain.Entities
{
    [Serializable]
    public class Model : BaseEntity
    {
        public static readonly string AppVersion = "FAP Alpha 5";
        public static readonly string ProtocolVersion = "FAP/1.0";
        public static int UPLINK_TIMEOUT = 60000;//1 minute
        public static int DOWNLOAD_RETRY_TIME = 120000;//2minutes
        public static int WEB_FREE_FILE_LIMIT = 524288;//0.5mb

        private readonly string oldSaveLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\FAP\Config.xml";
        private readonly string saveLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\FAP\ClientConfig.cfg";


        private SafeObservedCollection<Share> shares = new SafeObservedCollection<Share>();

        private SafeObservable<TransferSession> transferSessions = new SafeObservable<TransferSession>();
        private SafeObservedCollection<string> messages = new SafeObservedCollection<string>();

        private Network network = new Network();
        private int maxDownloads;
        private int maxDownloadsPerUser;
        private int maxUploads;
        private int maxUploadsPerUser;
        private string downloadFolder;
        private string incompleteFolder;
        private bool disableCompare;
        private string ipAddress;
        private bool alwaysNoCacheBrowsing;

        private OverlordPriority overlordPriority;

        private Node node;

        private DownloadQueue downloadQueue;
        // private SafeObservable<Session> sessions;

        //private Node node;
        // private ObservableCollection<Node> peers;

        //private SafeObservable<Fap.Network.Entity.Network> networks;
        //private SafeObservable<string> messages;
        // private SafeObservable<Conversation> converstations;
        //  private Overlord overlord;
        //  private PeerSortType peerSortType;
        // public delegate bool NewConversation(string id, string message);
        // public event NewConversation OnNewConverstation;

        /* public bool ReceiveConverstation(string id, string message)
         {
             if (null != OnNewConverstation)
                 return OnNewConverstation(id, message);
             return false;
         }
         */

        public Model()
        {
            node = new Node();
            downloadQueue = new DownloadQueue();
        }

        /// <summary>
        /// A flag to postpone shutdown set by operations which should not be interruped such as saving the download list.
        /// </summary>
        [JsonIgnore]
        public bool BlockShutdown { set; get; }

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

        public string Nickname
        {
            set
            {
                node.Nickname = value;
            }
            get
            {
                return node.Nickname;
            }
        }

        public string Description
        {
            set
            {
                node.Description = value;
            }
            get
            {
                return node.Description;
            }
        }

        public string Avatar
        {
            set
            {
                node.Avatar = value;
                NotifyChange("Avatar");
            }
            get
            {
                return node.Avatar;
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public DownloadQueue DownloadQueue
        {
            get { return downloadQueue; }
            set { downloadQueue = value; }
        }

        [XmlIgnore]
        [JsonIgnore]
        public Node LocalNode
        {
            set { node = value; NotifyChange("LocalNode"); }
            get { return node; }
        }

        public OverlordPriority OverlordPriority
        {
            set { overlordPriority = value; }
            get { return overlordPriority; }
        }

        [JsonIgnore]
        public Network Network
        {
            set { network = value; NotifyChange("Network"); }
            get { return network; }
        }

        public SafeObservedCollection<Share> Shares
        {
            set { shares = value; NotifyChange("Shares"); }
            get { return shares; }
        }


        public bool AlwaysNoCacheBrowsing
        {
            set { alwaysNoCacheBrowsing = value; NotifyChange("AlwaysNoCacheBrowsing"); }
            get { return alwaysNoCacheBrowsing; }
        }

        public string IPAddress
        {
            set { ipAddress = value; NotifyChange("IPAddress"); }
            get { return ipAddress; }
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
            get
            {
                return downloadFolder;
            }
        }

        public string IncompleteFolder
        {
            set
            {
                incompleteFolder = value;
                NotifyChange("IncompleteFolder");
            }
            get
            {
                return incompleteFolder;
            }
        }

        public bool DisableComparision
        {
            set
            {
                disableCompare = value;
            }
            get
            {
                return disableCompare;
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public SafeObservable<TransferSession> TransferSessions
        {
            get { return transferSessions; }
        }

        public int MaxDownloads
        {
            set
            {
                maxDownloads = value;
                NotifyChange("MaxDownloads");
            }
            get
            {
                return maxDownloads;
            }
        }

        public int MaxDownloadsPerUser
        {
            set
            {
                maxDownloadsPerUser = value;
                NotifyChange("MaxDownloadsPerUser");
            }
            get
            {
                return maxDownloadsPerUser;
            }
        }

        public int MaxUploads
        {
            set
            {
                maxUploads = value;
                NotifyChange("MaxUploads");
            }
            get
            {
                return maxUploads;
            }
        }

        public int MaxUploadsPerUser
        {
            set
            {
                maxUploadsPerUser = value;
                NotifyChange("maxUploadsPerUser");
            }
            get
            {
                return maxUploadsPerUser;
            }
        }

        public void Save()
        {
            lock (downloadQueue)
            {
                if (!Directory.Exists(Path.GetDirectoryName(saveLocation)))
                    Directory.CreateDirectory(Path.GetDirectoryName(saveLocation));
                File.WriteAllText(saveLocation, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
        }

        public void Load()
        {
            lock (downloadQueue)
            {
                try
                {
                    Model saved = null;
                    bool doneConvert = false;

                    //If the config file does not exist then check for the legacy format.
                    if (!File.Exists(saveLocation))
                    {
                        if (File.Exists(oldSaveLocation))
                        {
                            XmlSerializer deserializer = new XmlSerializer(typeof(Model));
                            using (TextReader textReader = new StreamReader(oldSaveLocation))
                            {
                                saved = (Model)deserializer.Deserialize(textReader);
                                doneConvert = true;
                            }
                        }
                    }
                    else
                    {
                        saved = JsonConvert.DeserializeObject<Model>(File.ReadAllText(saveLocation));
                    }

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
                    IPAddress = saved.IPAddress;
                    AlwaysNoCacheBrowsing = saved.AlwaysNoCacheBrowsing;

                    //Converted config so delete the old one
                    if (doneConvert)
                    {
                        Save();
                        // File.Delete(oldSaveLocation);
                    }
                }
                catch (Exception e)
                {
                    LogManager.GetLogger("faplog").WarnException("Failed to read config", e);
                }
            }
        }
    }
}