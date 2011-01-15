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
using Fap.Network.Services;
using Fap.Network.Entity;
using System.Threading;
using Fap.Foundation.Services;

namespace Fap.Network
{
    public class Client
    {
        private BufferService bufferService;
        private ConnectionService connectionService;

        public Client(BufferService bs, ConnectionService cs)
        {
            bufferService = bs;
            connectionService = cs;
        }


        public bool Execute(Request r, Node rc, out Response response)
        {
            var sess = connectionService.GetClientSession(rc);
            if (null == sess)
            {
                response = null;
                return false;
            }
            return Execute(r, sess, out response);
        }

        public bool Execute(Request r, Session session, out Response response)
        {
            try
            {
                byte[] data = Mediator.Serialize(r);
                session.Socket.ReceiveBufferSize = bufferService.Buffer;
                //transmit
                session.Socket.Send(data);
                //Receive
                var arg = bufferService.GetArg();

                ConnectionToken token = new ConnectionToken();
                int wait = 1;

                int startTime = Environment.TickCount;

                while (session.Socket.Connected)
                {
                    if (session.Socket.Available > 0)
                    {
                        int rx = session.Socket.Receive(arg.Data);
                        arg.SetDataLocation(0, rx);
                        token.ReceiveData(arg);
                        if (token.ContainsCommand())
                            break;
                        wait = 2;
                    }
                    else
                    {
                        //Time out
                        if (Environment.TickCount - startTime > 30000)
                        {
                            response = null;
                            session.Socket.Close();
                            connectionService.RemoveClientSession(session);
                            return false;
                        }
                        Thread.Sleep(wait);
                        if (wait < 200)
                            wait += 10;
                    }
                }
                Response resp = new Response();
                connectionService.FreeClientSession(session);
                if (Mediator.Deserialize(token.GetCommand(), out resp))
                {
                    response = resp;
                    return true;
                }
            }
            catch { }
            response = null;
            return false;
        }


        public bool Execute(IVerb cmd, Node rc)
        {
            return Execute(cmd, rc, IDService.CreateID());
        }

        public bool Execute(IVerb cmd, Node rc, string requestid)
        {
            var sess = connectionService.GetClientSession(rc);
            if (null == sess)
                return false;
            return Execute(cmd, sess, requestid);
        }

        public bool Execute(IVerb cmd, Session session, string requestid)
        {
            try
            {
                Response response = null;
                var request = cmd.CreateRequest();
                request.RequestID = requestid;
                if (Execute(request, session, out response))
                {
                    return cmd.ReceiveResponse(response);
                }
                return false;
            }
            catch
            {
                connectionService.FreeClientSession(session);
                return false;
            }
        }
    }
}
