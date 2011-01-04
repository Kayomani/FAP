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
using System.Net.Sockets;
using System.Net;
using Fap.Foundation;
using Fap.Network.Services;
using Fap.Foundation.Logging;
using Fap.Network.Entity;
using System.Threading;

namespace Fap.Network
{
    public class BroadcastClient : BroadcastCommon
    {
        private Socket listenSocket;
        private BufferService manager;
        private Logger logger;

        public delegate void BroadcastCommandRx(Request cmd);
        public event BroadcastCommandRx OnBroadcastCommandRx;

        public BroadcastClient(BufferService bs, Logger log)
        {
            manager = bs;
            logger = log;
        }

        private void ConnectListen()
        {
            if (null == listenSocket)
            {
                listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                listenSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(broadcastAddress, IPAddress.Any));
                listenSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, 1);
              //  listenSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface,);
                listenSocket.Bind(new IPEndPoint(IPAddress.Any, broadcastPort));

              //  listenSocket.Connect(broadcastAddress, broadcastPort);
                listenSocket.ReceiveBufferSize = manager.SmallBuffer;
                listenSocket.SendBufferSize = manager.SmallBuffer;
            }
        }

        public void StartListener()
        {
            if (null == listenSocket)
            {
                ConnectListen();
                QueueWork(new System.Waf.Applications.DelegateCommand(Run));
            }
        }


        private void Run()
        {
            while (listenSocket != null)
            {
                try
                {
                    var buff = manager.GetSmallArg();
                    buff.SetDataLocation(0, listenSocket.Receive(buff.Data));
                    if (buff.DataSize > 0)
                    {
                        ConnectionToken token = new ConnectionToken();
                        token.ReceiveData(buff);
                        if (token.ContainsCommand())
                        {
                            Request r = new Request();
                            string data = Encoding.Unicode.GetString(buff.Data,0,buff.DataSize);
                            if (Mediator.Deserialize(data, out r))
                            {
                              //  logger.AddInfo("Brocast client rx: " + r.Command + " P:" + r.Param);
                                if (null != OnBroadcastCommandRx)
                                    OnBroadcastCommandRx(r);
                            }
                        }
                    }
                }
                catch { }
            }
        }
    }
}
