using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using Fap.Foundation;
using NLog;

namespace FAP.Domain.Entities
{
    [Serializable]
    public class Model : BaseEntity
    {
        public static readonly string AppVersion = "FAP Alpha 5";
        public static readonly string ProtocolVersion = "FAP/1.0";
        public static int UPLINK_TIMEOUT = 60000;

        private readonly string saveLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\FAP\Config.xml";

        private BackgroundSafeObservable<Share> shares = new BackgroundSafeObservable<Share>();
       
        private SafeObservable<TransferSession> transferSessions = new SafeObservable<TransferSession>();
        public SafeObservedCollection<string> messages = new SafeObservedCollection<string>();

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

        // private DownloadQueue downloadQueue;
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
        }

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

        public Network Network
        {
            set { network = value; NotifyChange("Network"); }
            get { return network; }
        }

        public BackgroundSafeObservable<Share> Shares
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
            if (!Directory.Exists(Path.GetDirectoryName(saveLocation)))
                Directory.CreateDirectory(Path.GetDirectoryName(saveLocation));

            XmlSerializer serializer = new XmlSerializer(typeof(Model));
            using (TextWriter textWriter = new StreamWriter(saveLocation))
            {
                serializer.Serialize(textWriter, this);
                textWriter.Flush();
                textWriter.Close();
            }
        }

        public void Load()
        {
            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(Model));
                using (TextReader textReader = new StreamReader(saveLocation))
                {
                    Model m = (Model)deserializer.Deserialize(textReader);
                    textReader.Close();
                    Shares = m.Shares;
                    Avatar = m.Avatar;
                    Description = m.Description;
                    Nickname = m.Nickname;
                    DownloadFolder = m.DownloadFolder;
                    MaxDownloads = m.MaxDownloads;
                    MaxDownloadsPerUser = m.MaxDownloadsPerUser;
                    MaxUploads = m.MaxUploads;
                    MaxUploadsPerUser = m.MaxUploadsPerUser;
                    //   MaxOverlordPeers = m.MaxOverlordPeers;
                    //  LocalNodeID = m.LocalNodeID;
                    DisableComparision = m.DisableComparision;
                    IPAddress = m.IPAddress;
                    AlwaysNoCacheBrowsing = m.AlwaysNoCacheBrowsing;
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger("faplog").WarnException("Failed to read config", e);
            }
        }
    }
}