#region Copyright Kayomani 2010.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
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
using System.Threading;
using Fap.Foundation;
using Fap.Network.Services;
using System.Net.Sockets;

namespace Fap.Network.Entity
{
    /// <summary>
    /// Handles ordered bi direction communications between nodes
    /// </summary>
    public class Uplink
    {
        //Timeout checking
        private long timeoutPeriod = 80000;
        private long lastRx = Environment.TickCount;
        private long lastTx = Environment.TickCount;

          //On uplink disconnection
        public delegate void Disconnect(Uplink s);
        public event Disconnect OnDisconnect;

        private bool running = true;

        // --------------------- TX ---------------------
        private BackgroundSafeObservable<Request> pendingRequests = new BackgroundSafeObservable<Request>();
        private Node node;
        private Session session;
        //Fired upon tx connection timing out, requesting an request to send to stop the timeout
        public delegate Request TxTimingout();
        public event TxTimingout OnTxTimingout;
        //Worker stuff
        private AutoResetEvent workerEvent = new AutoResetEvent(true);
        // --------------------- RX ---------------------
        private FapConnectionHandler rxHandler;

        public event FapConnectionHandler.ReceiveRequest OnReceivedRequest;

        public Uplink(Node n, Session s, BufferService b)
        {
            node = n;
            session = s;
            rxHandler = new FapConnectionHandler(b);
            rxHandler.OnDisconnect += new FapConnectionHandler.Disconnect(rxHandler_OnDisconnect);
            rxHandler.OnReceiveRequest += new FapConnectionHandler.ReceiveRequest(rxHandler_OnReceiveRequest);
           
        }

        public void Start(Socket rxSocket)
        {
            lastRx = Environment.TickCount;
            lastTx = Environment.TickCount;
            ThreadPool.QueueUserWorkItem(new WaitCallback(Process));
            ThreadPool.QueueUserWorkItem(new WaitCallback(ListenAsync), rxSocket);
        }

        private void ListenAsync(object o)
        {
            Socket s = o as Socket;
            if(null!=s)
                rxHandler.HandleConnection(s);
        }


        /// <summary>
        /// Incoming commands from the server.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        private FAPListenerRequestReturnStatus rxHandler_OnReceiveRequest(Request r, Socket s)
        {
            if (null != OnReceivedRequest)
                return OnReceivedRequest(r, s);
            throw new Exception("Received command with no active listener");
        }

        private void rxHandler_OnDisconnect()
        {
            Kill();
        }
       
        public bool Running
        {
            get { return running; }
        }

        public Node Node
        {
            get { return node; }
        }

        public long TimeoutPeriod
        {
            set { timeoutPeriod = value; }
            get { return timeoutPeriod; }
        }

        public int PendingRequests
        {
            get { return pendingRequests.Count; }
        }

        public bool AddMessage(Request r)
        {
            if (running)
                pendingRequests.Add(r);
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
                        Request r = pendingRequests[0];
                        pendingRequests.RemoveAt(0);
                        r.RequestID = node.Secret;
                        session.Socket.Send(Mediator.Serialize(r));
                        node.LastUpdate = Environment.TickCount;
                        lastTx = Environment.TickCount;
                    }

                    long now = Environment.TickCount;
                    //Check for tx time out
                    if (lastTx + timeoutPeriod < now)
                    {
                        //Tx timeout - We shouldn't really ever get here!
                        running = false;
                        break;
                    }
                    else if (lastTx + (timeoutPeriod * 0.8) < now)
                    {
                        if (null != OnTxTimingout)
                        {
                            AddMessage(OnTxTimingout());
                            workerEvent.Set();
                        }
                    }
                    //Wait until there is work to do or 5 seconds have elapsed
                    workerEvent.WaitOne(5000);
                }
            }
            catch { }
            finally
            {
                if (null != OnDisconnect)
                    OnDisconnect(this);
                session.Dispose();
            }
        }
    }
}
