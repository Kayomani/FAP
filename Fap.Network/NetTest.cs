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
using Fap.Network.Entity;
using Fap.Foundation;

namespace Fap.Network
{
    public class NetTest
    {

        public void Test()
        {
            Request r = new Request();
            r.Command = "GET";
            r.Param = "test.file";
            r.RequestID = "id";
            r.AdditionalHeaders.Add("test", "aaa");
            r.ContentSize = 1234;

            byte[] data = Mediator.Serialize(r);
            MemoryBuffer mb=  new MemoryBuffer(data.Length);
            mb.Data = data;
            mb.SetDataLocation(0, data.Length);

            ConnectionToken token = new ConnectionToken();
            token.ReceiveData(mb);
          
            if (token.ContainsCommand())
            {
                Request i = new Request();
                if (Mediator.Deserialize(token.GetCommand(), out i))
                {

                }
            }
        }
    }
}
