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
using Fap.Network;
using Fap.Network.Entity;
using Fap.Domain.Entity;

namespace Fap.Domain.Verbs
{
    public class InfoVerb : VerbBase, IVerb
    {
        private Node model;

        public InfoVerb(Node m)
        {
            model = m;
        }

        public Request CreateRequest()
        {
            Request r = new Request();
            r.Command = "INFO";
            r.Param = model.ID;
            r.RequestID = model.Secret;
            return r;
        }

        public Network.Entity.Response ProcessRequest(Network.Entity.Request r)
        {
            Response response = new Response();
            response.Status = 0;
            response.RequestID = r.RequestID;
            foreach (var data in model.Data)
            {
                if (!string.IsNullOrEmpty(data.Key))
                    response.AdditionalHeaders.Add(data.Key, data.Value);
            }
            return response;
        }

        public bool ReceiveResponse(Network.Entity.Response r)
        {
            try
            {
                if (r.Status == 0 && r.ContentSize == 0)
                {
                    //If we don't have the full specification then fail the response.
                    foreach (var data in r.AdditionalHeaders)
                        model.SetData(data.Key, data.Value);
                    return true;
                }
            }
            catch { }
            return false;
        }

        public Node Model
        {
            set { model = value; }
            get { return model; }
        }
    }
}
