using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAP.Network.Services;
using Autofac;
using FAP.Domain.Verbs;
using Fap.Foundation;

namespace FAP.Domain.Net
{
    public class LANPeerFinderService
    {
        private BackgroundSafeObservable<DetectedNode> announcedAddresses = new BackgroundSafeObservable<DetectedNode>();
        private MulticastClientService mclient;
        private IContainer container;

        public LANPeerFinderService(IContainer c)
        {
            container = c;
            announcedAddresses.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(announcedAddresses_CollectionChanged);
        }

        void announcedAddresses_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {

            }
        }

        public List<DetectedNode> Peers
        {
            get { return announcedAddresses.ToList(); }
        }

        public void RemovePeer(DetectedNode d)
        {
            announcedAddresses.Lock();
            if (announcedAddresses.Contains(d))
                announcedAddresses.Remove(d);
            announcedAddresses.Unlock();
        }

        public void Start()
        {
            lock (announcedAddresses)
            {
                if (null == mclient)
                {
                    mclient = container.Resolve<MulticastClientService>();
                    mclient.OnMultiCastRX += new MulticastClientService.MultiCastRX(mclient_OnMultiCastRX);
                    mclient.StartListener();
                }
            }
        }

        private void mclient_OnMultiCastRX(string cmd)
        {
            if (cmd.StartsWith(HelloVerb.Preamble))
            {
                HelloVerb verb = new HelloVerb();
                var node = verb.ParseRequest(cmd);
                if (null != node)
                {
                    var search = announcedAddresses.Where(s => s.Address == node.Address).FirstOrDefault();
                    if (null == search)
                    {
                        node.LastAnnounce = DateTime.Now;
                        announcedAddresses.Add(node);
                        
                    }
                    else
                    {
                        search.LastAnnounce = DateTime.Now;
                        search.OverlordID = node.OverlordID;
                        search.NetworkName = node.NetworkName;
                        search.NetworkID = node.NetworkID;
                        search.Priority = node.Priority;
                        search.CurrentUsers = node.CurrentUsers;
                        search.MaxUsers = node.MaxUsers;
                    }
                }
            }
        }
    }
}
