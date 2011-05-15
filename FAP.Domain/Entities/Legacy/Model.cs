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
using System.Xml.Serialization;
using System.IO;
using Fap.Foundation;
using System.Collections.ObjectModel;

namespace FAP.Domain.Entities.Legacy
{
    /// <summary>
    /// Represents a pre alpha 5 model.
    /// </summary>
    [Serializable]
    public class Model : INotifyPropertyChanged
    {
        public static readonly string saveLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\FAP\Config.xml";
        private DownloadQueue downloadQueue;
        private int maxDownloads;
        private int maxDownloadsPerUser;
        private int maxUploads;
        private int maxUploadsPerUser;
        private string downloadFolder;
        private string incompleteFolder;
        private bool disableCompare;
        private string ipAddress;
        private bool alwaysNoCacheBrowsing;

        private SafeObservable<TransferSession> transferSessions;
        private Node node;
        private ObservableCollection<Node> peers;
        private SafeObservable<Share> shares;
        private SafeObservable<Network> networks;
        private SafeObservable<string> messages;
        private SafeObservable<Conversation> converstations;
        private Overlord overlord;
        private PeerSortType peerSortType;

        public delegate bool NewConversation(string id, string message);
        public event NewConversation OnNewConverstation;

        public bool ReceiveConverstation(string id, string message)
        {
            if (null != OnNewConverstation)
                return OnNewConverstation(id, message);
            return false;
        }

        public Model()
        {
            peers = new ObservableCollection<Node>();
            shares = new SafeObservable<Share>();
            messages = new SafeObservable<string>();
            converstations = new SafeObservable<Conversation>();
            transferSessions = new SafeObservable<TransferSession>();
            node = new Node();
            overlord = new Overlord();
        }

      

        public PeerSortType PeerSortType
        {
            set { peerSortType = value; NotifyChange("PeerSortType"); }
            get { return peerSortType; }
        }

        public bool AlwaysNoCacheBrowsing
        {
            set { alwaysNoCacheBrowsing = value; NotifyChange("AlwaysNoCacheBrowsing"); }
            get { return alwaysNoCacheBrowsing; }
        }

        public SafeObservable<Share> Shares
        {
            set { shares = value; NotifyChange("Shares"); }
            get { return shares; }
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


        public int MaxOverlordPeers
        {
            set
            {
                overlord.MaxPeers = value;
                NotifyChange("MaxOverlordPeers");
            }
            get
            {
                return overlord.MaxPeers;
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

        /// <summary>
        /// Copy of the id saved in the config file
        /// </summary>
        public string LocalNodeID
        {
            set
            {
                node.ID = value;
                NotifyChange("LocalNodeID");
            }
            get
            {
                return node.ID;
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

        public long TotalShareSize
        {
            get
            {
                return node.ShareSize;
            }
            set
            {
                node.ShareSize = value;
                NotifyChange("TotalShareSize");
            }
        }

        [XmlIgnore]
        public Node Node
        {
            get { return node; }
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
                    MaxOverlordPeers = m.MaxOverlordPeers;
                    LocalNodeID = m.LocalNodeID;
                    DisableComparision = m.DisableComparision;
                    IPAddress = m.IPAddress;
                    AlwaysNoCacheBrowsing = m.AlwaysNoCacheBrowsing;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to read config", e);
            }
        }

        private void NotifyChange(string path)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(path));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
