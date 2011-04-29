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
using System.Net.Sockets;
using NLog;
using System.Net;
using System.Threading;

namespace FAP.Network.Services
{
    public class MulticastServerService : MulticastCommon
    {
        private Socket broadcastSocket;
        private Logger logService;

        private byte[] message;
        private bool run = false;

        private AutoResetEvent workerEvent = new AutoResetEvent(true);
        private List<string> announceQueue = new List<string>();

        public MulticastServerService()
        {
            logService = LogManager.GetLogger("faplog");
        }

        private void ConnectBroadcast()
        {
            broadcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            broadcastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(broadcastAddress, IPAddress.Any));
            broadcastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, 1);
            broadcastSocket.Connect(broadcastAddress, broadcastPort);
        }

        public void Start(string cmd)
        {
            message = Encoding.Unicode.GetBytes(cmd);
            CheckStart();
        }

        private void CheckStart()
        {
            if (!run)
            {
                run = true;
                ConnectBroadcast();
                ThreadPool.QueueUserWorkItem(new WaitCallback(DoWork));
            }
        }

        public void AddMessage(string msg)
        {
            lock (announceQueue)
                announceQueue.Add(msg);
            CheckStart();
        }

        public void TriggerAnnounce()
        {
            workerEvent.Set();
        }

        public void Stop()
        {
            if (run)
            {
                run = false;
                broadcastSocket.Close();
            }
        }

        private void DoWork(object o)
        {
            while (run)
            {
                lock (announceQueue)
                {
                    while (announceQueue.Count > 0)
                    {
                        broadcastSocket.SendTo(Encoding.Unicode.GetBytes(announceQueue[0]), broadcastSocket.RemoteEndPoint);
                        announceQueue.RemoveAt(0);
                    }
                }
                if (message != null && message.Length!=0)
                    broadcastSocket.SendTo(message, broadcastSocket.RemoteEndPoint);
                workerEvent.WaitOne(10000);
            }
        }
    }
}
