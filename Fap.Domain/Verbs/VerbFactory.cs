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
    public class VerbFactory
    {
        public IVerb GetVerb(string name, Model n)
        {
            switch (name)
            {
                case "INFO":
                    return new InfoVerb(n.Node);
                case "CONNECT":
                    return new ConnectVerb(n.Node);
               // case "BROWSE":
                //    return new BrowseVerb(n);
                case "COMPARE":
                    return new CompareVerb(n);
                case "CONVERSATION":
                    return new ConversationVerb(n);
                case "PING":
                    return new PingVerb(n.Node);
                case "NOOP":
                    return new NoopVerb();
                default:
                    throw new Exception("Unknown command");
            }
        }
    }
}
