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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace FAP.Domain.Entities
{
    public class NetInterface
    {
        private string name;
        private long speed;
        private string description;
        private IPAddress address;

        public string Name
        {
            set { name = value; }
            get { return name; }
        }

        public long Speed
        {
            set { speed = value; }
            get { return speed; }
        }

        public string Description
        {
            set { description = value; }
            get { return description; }
        }

        public IPAddress Address
        {
            set { address = value; }
            get { return address; }
        }
    }
}
