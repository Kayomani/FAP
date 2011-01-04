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
using Fap.Domain.Commands;
using Fap.Domain.Entity;
using System.Net.Sockets;
using Fap.Domain.Services;
using Fap.Network.Services;
using Fap.Network.Entity;

namespace Fap.Domain
{
   /* public class Client
    {
        private BufferService bufferService;
        private ConnectionService connectionService;

        public Client(BufferService bs, ConnectionService cs)
        {
            bufferService = bs;
            connectionService = cs;
        }

        public bool Execute(ICommsCommand cmd, RemoteClient rc)
        {
            var sess = connectionService.GetClientSession(rc);
            if (null == sess)
                return false;
            return Execute(cmd, sess);
        }

        public bool Execute(ICommsCommand cmd,Session session)
        {
            try
            {
                string[] c = cmd.CreateRequest(session);
                Mediator m = new Mediator();
                //transmit
                string tx = Mediator.Serialise(c);
                session.Socket.Send(ASCIIEncoding.ASCII.GetBytes(tx));
                //Receive
                var arg = bufferService.GetArg();
                session.Socket.ReceiveBufferSize = arg.Data.Length;

                StringBuilder sb = new StringBuilder();
                while (true)
                {
                    if (session.Socket.Available == 0)
                        if (Mediator.ContainsCommand(sb.ToString()))
                            break;
                    session.Socket.Blocking = true;
                    int rx = session.Socket.Receive(arg.Data);
                    sb.Append(ASCIIEncoding.ASCII.GetString(arg.Data, 0, rx));
                }
                bufferService.FreeArg(arg);
                string[] rx2 = Mediator.Deserialize(sb.ToString());
                connectionService.FreeClientSession(session);
                return cmd.ReceiveResponse(session, rx2);
            }
            catch
            {
                connectionService.FreeClientSession(session);
                return false;
            }
        }
    }*/
}
