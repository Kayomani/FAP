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
using System.Threading;
using FAP.Domain.Entities;
using Fap.Foundation;
using FAP.Network.Entities;

namespace FAP.Domain.Net
{
    /// <summary>
    /// Handles sending updates to a client in a queue and managing client timeouts.
    /// </summary>
    public class ClientStream
    {
        #region Delegates

        public delegate void Disconnect(ClientStream s);

        #endregion

        public readonly int PRE_TIMEOUT_PERIOD = 10000;

        private readonly BackgroundSafeObservable<NetworkRequest> pendingRequests =
            new BackgroundSafeObservable<NetworkRequest>();

        private readonly AutoResetEvent workerEvent = new AutoResetEvent(true);
        private Node destination;
        private bool run = true;
        private Node serverNode;

        public Node Node
        {
            get { return destination; }
        }

        public void Start(Node _destination, Node _serverNode)
        {
            destination = _destination;
            serverNode = _serverNode;
            destination.LastUpdate = Environment.TickCount;
            ThreadPool.QueueUserWorkItem(Process);
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
                pendingRequests.Add(new NetworkRequest {Verb = verb, Param = param, Data = data});
                workerEvent.Set();
            }
        }

        public void AddMessage(NetworkRequest r)
        {
            if (run)
            {
                pendingRequests.Add(r.Clone());
                workerEvent.Set();
            }
        }

        //On uplink disconnection

        public event Disconnect OnDisconnect;

        private void Process(object o)
        {
            try
            {
                while (run)
                {
                    //If the client has timed out then disconect
                    if (pendingRequests.Count == 0 &&
                        Environment.TickCount - destination.LastUpdate > Model.UPLINK_TIMEOUT)
                    {
                        if (null != OnDisconnect)
                            OnDisconnect(this);
                        var req = new NetworkRequest {Verb = "DISCONNECT", SourceID = serverNode.ID};
                        TransmitRequest(req);
                        return;
                    }
                    //If the client is going to timeout in the next 15 seconds then do an update
                    if (pendingRequests.Count == 0 &&
                        Environment.TickCount - destination.LastUpdate > Model.UPLINK_TIMEOUT - PRE_TIMEOUT_PERIOD)
                        pendingRequests.Add(new NetworkRequest
                                                {
                                                    Data = string.Empty,
                                                    Param = string.Empty,
                                                    Verb = "NOOP",
                                                    SourceID = serverNode.ID
                                                });

                    while (pendingRequests.Count > 0)
                    {
                        NetworkRequest req = pendingRequests[0];
                        TransmitRequest(req);
                        pendingRequests.RemoveAt(0);
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
            //AutoResetEvent w = workerEvent;
            //workerEvent = null;
            //w.Close();
            pendingRequests.Clear();
        }

        private void TransmitRequest(NetworkRequest req)
        {
            var client = new Client(serverNode);
            req.AuthKey = destination.Secret;
            if (!client.Execute(req, destination))
                throw new Exception("Transmission failiure");
            destination.LastUpdate = Environment.TickCount;
        }
    }
}