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

using System.Net;
using System.Net.Sockets;
using System.Text;
using NLog;

namespace FAP.Network.Services
{
    public class MulticastServerService : MulticastCommon
    {
        private readonly object sync = new object();
        private Socket broadcastSocket;
        private Logger logService;

        public MulticastServerService()
        {
            logService = LogManager.GetLogger("faplog");
        }

        private void ConnectBroadcast()
        {
            broadcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            broadcastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
                                            new MulticastOption(broadcastAddress, IPAddress.Any));
            broadcastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, 1);
            broadcastSocket.Connect(broadcastAddress, broadcastPort);
        }

        public void SendMessage(string msg)
        {
            lock (sync)
            {
                if (null == broadcastSocket)
                    ConnectBroadcast();
                broadcastSocket.SendTo(Encoding.UTF8.GetBytes(msg), broadcastSocket.RemoteEndPoint);
            }
        }

        public void Stop()
        {
            if (null != broadcastSocket)
            {
                broadcastSocket.Close();
                broadcastSocket = null;
            }
        }
    }
}