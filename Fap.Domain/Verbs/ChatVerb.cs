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

namespace Fap.Domain.Verbs
{
    public class ChatVerb : VerbBase, IVerb
    {

        public Network.Entity.Request CreateRequest()
        {
            Network.Entity.Request r = new Request();
            r.AdditionalHeaders.Add("Name", Nickname);
            r.AdditionalHeaders.Add("Text", Message);
            r.Param = SourceID;
            r.Command = "CHAT";
            return r;
        }

        public Network.Entity.Response ProcessRequest(Network.Entity.Request r)
        {
            Nickname = GetValueSafe(r.AdditionalHeaders, "Name");
            Message = GetValueSafe(r.AdditionalHeaders, "Text");
            SourceID = r.Param;
            return null;
        }

        public bool ReceiveResponse(Network.Entity.Response r)
        {
            throw new NotImplementedException();
        }

        public string Nickname { set; get; }
        public string Message { set; get; }
        public string SourceID { set; get; }
    }
}
