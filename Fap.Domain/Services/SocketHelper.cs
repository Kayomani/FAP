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
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace Fap.Domain.Services
{
    public class SocketHelper
    {
        /// <summary>
        /// Finds the next availible port starting from paramter 1.
        /// </summary>
        /// <param name="nport">Starting port number</param>
        /// <param name="theInterface">Interace on which the port will be used.</param>
        /// <param name="protocol">Protocol</param>
        /// <returns></returns>
        public static int findAvailiblePort(int nport, IPAddress theInterface, ProtocolType protocol)
        {
            try
            {
                IPEndPoint ip = new IPEndPoint(theInterface, nport);
                switch (protocol)
                {
                    case ProtocolType.Tcp:
                        Socket socketToTest = socketToTest = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socketToTest.Bind(ip);
                        socketToTest.Close();
                        break;
                    case ProtocolType.Udp:
                        UdpClient udpc = new UdpClient(ip);
                        udpc.Close();
                        break;
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("Port not availible:" + nport);
                if (nport > 64000)
                    throw new Exception(e.ToString());
                nport++;
                return findAvailiblePort(nport, theInterface, protocol);
            }
            return nport;
        }
    }
}
