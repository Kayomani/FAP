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

using FAP.Network.Entities;

namespace FAP.Domain.Verbs
{
    public class ChatVerb : BaseVerb, IVerb
    {
        public string Nickname { set; get; }
        public string Message { set; get; }
        public string SourceID { set; get; }

        #region IVerb Members

        public NetworkRequest CreateRequest()
        {
            var req = new NetworkRequest();
            req.Data = Serialize(this);
            req.Verb = "CHAT";
            return req;
        }

        public NetworkRequest ProcessRequest(NetworkRequest r)
        {
            ReceiveResponse(r);
            return CreateRequest();
        }

        public bool ReceiveResponse(NetworkRequest r)
        {
            var inc = Deserialise<ChatVerb>(r.Data);
            Nickname = inc.Nickname;
            Message = inc.Message;
            SourceID = inc.SourceID;
            return true;
        }

        #endregion
    }
}