using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Autofac;
using FAP.Domain.Verbs;
using Fap.Foundation;
using FAP.Network.Services;

namespace FAP.Domain.Net
{
    public class LANPeerFinderService
    {
        private readonly BackgroundSafeObservable<DetectedNode> announcedAddresses =
            new BackgroundSafeObservable<DetectedNode>();

        private readonly IContainer container;
        private MulticastClientService mclient;

        public LANPeerFinderService(IContainer c)
        {
            container = c;
            announcedAddresses.CollectionChanged += announcedAddresses_CollectionChanged;
        }

        public List<DetectedNode> Peers
        {
            get { return announcedAddresses.ToList(); }
        }

        private void announcedAddresses_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add)
            {
            }
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
                    mclient.OnMultiCastRX += mclient_OnMultiCastRX;
                    mclient.StartListener();
                }
            }
        }

        private void mclient_OnMultiCastRX(string cmd)
        {
            if (cmd.StartsWith(HelloVerb.Preamble))
            {
                var verb = new HelloVerb();
                DetectedNode node = verb.ParseRequest(cmd);
                if (null != node)
                {
                    DetectedNode search = announcedAddresses.Where(s => s.Address == node.Address).FirstOrDefault();
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