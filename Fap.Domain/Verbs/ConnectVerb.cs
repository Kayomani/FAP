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
using Fap.Foundation.Services;

namespace Fap.Domain.Verbs
{
    public class ConnectVerb : VerbBase, IVerb
    {
        public Network.Entity.Request CreateRequest()
        {
            Request r = new Request();
            r.Command = "CONNECT";
            r.Param = RemoteLocation;
            r.RequestID = IDService.CreateID();
            return r;
        }

        public Network.Entity.Response ProcessRequest(Network.Entity.Request r)
        {
            throw new NotImplementedException();
        }

        public bool ReceiveResponse(Network.Entity.Response r)
        {
            Status = r.Status;
            NetworkID = GetValueSafe(r.AdditionalHeaders, "ID");
            OverlordID = GetValueSafe(r.AdditionalHeaders, "Host");
            Name = GetValueSafe(r.AdditionalHeaders, "Name");
            Secret = r.RequestID;
            return true;
        }

        public string RemoteLocation { set; get; }
        public string OverlordID { set; get; }
        public string Name { set; get; }
        public string NetworkID { set; get; }
        public string Secret { set; get; }
    }
}
