using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Fap.Foundation;

namespace Fap.Network.Entity
{
    public class PeerStream
    {
        private SafeObservable<Request> pendingRequests = new SafeObservable<Request>();
        private Node node;
        private bool running = true;
        private Session session;
        private Thread worker;

        public delegate void Disconnect(PeerStream s);
        public event Disconnect OnDisconnect;

        public delegate void OverlordConnectionTimingout(PeerStream s);
        public event OverlordConnectionTimingout OnOverlordConnectionTimingout;

        public bool Running
        {
            get { return running; }
        }

        public Node Node
        {
            get { return node; }
        }

        public int PendingRequests
        {
            get { return pendingRequests.Count; }
        }

        public PeerStream(Node n, Session s)
        {
            node = n;
            session = s;
            ThreadPool.QueueUserWorkItem(new WaitCallback(Process));
        }

        public bool AddMessage(Request r)
        {
            if (running)
            {
                pendingRequests.Add(r);
            }
            return running;
        }

        public void Kill()
        {
            running = false;
        }


        private void Process(object o)
        {
            int sleep = 25;
            worker = Thread.CurrentThread;
            try
            {
                while (running)
                {
                    if (pendingRequests.Count > 0)
                    {
                        sleep = 25;
                        Request r = pendingRequests[0];
                        pendingRequests.RemoveAt(0);
                        r.RequestID = node.Secret;
                        session.Socket.Send(Mediator.Serialize(r));
                        node.LastUpdate = Environment.TickCount;
                    }
                    else
                    {
                        //Check for client timeout
                        if (Environment.TickCount - node.LastUpdate > 60000 && node.NodeType != ClientType.Overlord)
                        {
                            Kill();
                            if (null != OnDisconnect)
                                OnDisconnect(this);
                        }
                        else if (node.NodeType == ClientType.Overlord && Environment.TickCount - node.LastUpdate > 45000)
                        {
                            //Don't allow interoverlord comms to time out
                            if(OnOverlordConnectionTimingout!=null)
                                OnOverlordConnectionTimingout();
                        }

                        if (sleep < 350)
                            sleep += 20;
                        Thread.Sleep(sleep);
                    }
                }
            }
            catch
            {
                if (null != OnDisconnect)
                    OnDisconnect(this);
            }
        }
    }
}
