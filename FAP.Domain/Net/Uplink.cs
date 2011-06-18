using System;
using System.Threading;
using FAP.Domain.Entities;
using Fap.Foundation;
using FAP.Network.Entities;

namespace FAP.Domain.Net
{
    /// <summary>
    /// Handles ordered updates upto a server.
    /// </summary>
    public class Uplink
    {
        //On uplink disconnection

        #region Delegates

        public delegate void Disconnect(Uplink s);

        #endregion

        private readonly Node destination;

        // --------------------- TX ---------------------
        private readonly BackgroundSafeObservable<NetworkRequest> pendingRequests =
            new BackgroundSafeObservable<NetworkRequest>();

        private readonly Node source;

        //Worker stuff
        private readonly AutoResetEvent workerEvent = new AutoResetEvent(true);
        private bool running = true;

        public Uplink(Node source, Node destination)
        {
            this.source = source;
            this.destination = destination;
        }

        public bool Running
        {
            get { return running; }
        }

        public Node Source
        {
            get { return source; }
        }

        public Node Destination
        {
            get { return destination; }
        }

        public int PendingRequests
        {
            get { return pendingRequests.Count; }
        }

        public event Disconnect OnDisconnect;

        public void Start()
        {
            ThreadPool.QueueUserWorkItem(Process);
        }

        public void RecieveCommand()
        {
            destination.LastUpdate = Environment.TickCount;
        }

        public bool AddMessage(NetworkRequest r)
        {
            if (running)
                pendingRequests.Add(r);
            workerEvent.Set();
            return running;
        }

        public void Kill()
        {
            running = false;
            workerEvent.Set();
        }

        private void Process(object o)
        {
            try
            {
                while (running)
                {
                    while (pendingRequests.Count > 0)
                    {
                        NetworkRequest req = pendingRequests[0];
                        pendingRequests.RemoveAt(0);

                        destination.LastUpdate = Environment.TickCount;
                        var c = new Client(source);
                        if (!c.Execute(req, destination))
                        {
                            //Error
                            running = false;
                            return;
                        }
                    }

                    //Check for client time out
                    if ((Environment.TickCount - destination.LastUpdate) > Model.UPLINK_TIMEOUT)
                    {
                        //We havent recently sent/recieved so went a noop so check we are still connected.
                        var req = new NetworkRequest {Verb = "NOOP", SourceID = source.ID, AuthKey = destination.Secret};
                        var client = new Client(source);
                        if (!client.Execute(req, destination, 4000))
                        {
                            //Error
                            running = false;
                            return;
                        }
                    }

                    //Wait until there is work to do or 5 seconds have elapsed
                    workerEvent.WaitOne(5000);
                }
            }
            catch
            {
            }
            finally
            {
                if (null != OnDisconnect)
                    OnDisconnect(this);
            }
        }
    }
}