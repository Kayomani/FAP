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
            message = Encoding.ASCII.GetBytes(cmd);
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
                        broadcastSocket.SendTo(Encoding.ASCII.GetBytes(announceQueue[0]), broadcastSocket.RemoteEndPoint);
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
