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
using FAP.Domain.Entities;
using System.Threading;
using Fap.Foundation;
using FAP.Network.Entities;

namespace FAP.Domain.Net
{
    /// <summary>
    /// Handles sending updates to a client in a queue and managing client timeouts.
    /// </summary>
    public class ClientStream
    {
        private BackgroundSafeObservable<NetworkRequest> pendingRequests = new BackgroundSafeObservable<NetworkRequest>();
        private Node node;
        private Node server;

        private AutoResetEvent workerEvent = new AutoResetEvent(true);
        private bool run = true;

        public Node Node
        {
            get { return node; }
        }

        public void Start(Node destination, Node serverNode)
        {
            node = destination;
            server = serverNode;
            destination.LastUpdate = Environment.TickCount;
            ThreadPool.QueueUserWorkItem(new WaitCallback(Process));
        }

        public void Kill()
        {
            run = false;
            if (null != workerEvent)
                workerEvent.Set();
        }

        public void AddMessage(string verb, string param, string data)
        {
            if (run)
            {
                pendingRequests.Add(new NetworkRequest() { Verb = verb, Param = param, Data = data });
                workerEvent.Set();
            }
        }

        public void AddMessage(NetworkRequest r)
        {
            if (run)
            {
                pendingRequests.Add(r);
                workerEvent.Set();
            }
        }

        //On uplink disconnection
        public delegate void Disconnect(ClientStream s);
        public event Disconnect OnDisconnect;

        private void Process(object o)
        {
            try
            {
                while (run)
                {
                    //If the client has timed out then disconect
                    if (pendingRequests.Count == 0 && Environment.TickCount - node.LastUpdate > Model.UPLINK_TIMEOUT)
                    {
                        if (null != OnDisconnect)
                            OnDisconnect(this);
                        NetworkRequest req = new NetworkRequest() { Verb = "DISCONNECT" };
                        TransmitRequest(req);
                        return;
                    }
                    //If the client is going to timeout in the next 15 seconds then do an update
                    if (pendingRequests.Count == 0 && Environment.TickCount - node.LastUpdate > Model.UPLINK_TIMEOUT - 15000)
                        pendingRequests.Add(new NetworkRequest() { Data = string.Empty, Param = string.Empty, Verb = "NOOP" });

                    while (pendingRequests.Count > 0)
                    {
                        NetworkRequest req = pendingRequests[0];
                        pendingRequests.RemoveAt(0);
                        TransmitRequest(req);
                    }
                    workerEvent.WaitOne(5000);
                }
            }
            catch
            {
                if (null != OnDisconnect)
                    OnDisconnect(this);
            }
            //Clean up
            AutoResetEvent w = workerEvent;
            workerEvent = null;
            w.Close();
            pendingRequests.Clear();
            node = null;
        }

        private void TransmitRequest(NetworkRequest req)
        {
            Client client = new Client(server);

            if (!client.Execute(req, node))
                throw new Exception("Transmission failiure");
            node.LastUpdate = Environment.TickCount;
        }
    }
}
