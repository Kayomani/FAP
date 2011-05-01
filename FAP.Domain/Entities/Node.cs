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
using Fap.Foundation;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace FAP.Domain.Entities
{
    public class Node : BaseEntity
    {
        protected SafeDictionary<string, string> data = new SafeDictionary<string, string>();
        private long lastUpdate = Environment.TickCount;

        
        private object sync;
        private string secret;


        public override bool Equals(object obj)
        {
            Node other = obj as Node;
            if (null != other)
            {
                return other.ID == ID;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (null == ID)
                return 0;
            return ID.GetHashCode();
        }

        public Node()
        {
            sync = new object();
            LastUpdate = Environment.TickCount;
        }

        [JsonIgnoreAttribute]
        public string Secret
        {
            get { return secret; }
            set
            {
                secret = value;
                LastUpdate = Environment.TickCount;
                NotifyChange("secret");
            }
        }

        [JsonIgnoreAttribute]
        public string OverlordID
        {
            get { return data.SafeGet("OverlordID"); }
            set
            {
                data.Set("OverlordID", value);
                LastUpdate = Environment.TickCount;
                NotifyChange("OverlordID");
            }
        }

        [JsonIgnoreAttribute]
        public bool Online
        {
            get
            {
                bool d = false;
                bool.TryParse(data.SafeGet("Online"), out d);
                return d;
            }
            set
            {
                data.Set("Online", value.ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("Online");
            }
        }

        [JsonIgnoreAttribute]
        public long LastUpdate
        {
            set
            {
                lock (sync)
                    lastUpdate = value;
                NotifyChange("LastUpdate");
            }
            get
            {
                lock (sync)
                    return lastUpdate;
            }
        }

        [JsonIgnoreAttribute]
        public string ClientVersion
        {
            get { return data.SafeGet("ClientVersion"); }
            set
            {
                data.Set("ClientVersion", value);
                LastUpdate = Environment.TickCount;
                NotifyChange("ClientVersion");
            }
        }

        [JsonIgnoreAttribute]
        public string ID
        {
            get { return data.SafeGet("ID"); }
            set
            {
                data.Set("ID", value);
                LastUpdate = Environment.TickCount;
                NotifyChange("ID");
            }
        }

        [JsonIgnoreAttribute]
        public ClientType NodeType
        {
            get
            {
                int i = 0;
                int.TryParse(data.SafeGet("NodeType"), out i);
                return (ClientType)i;
            }
            set
            {
                data.Set("NodeType", ((int)value).ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("NodeType");
            }
        }

        [JsonIgnoreAttribute]
        public long DownloadSpeed
        {
            get
            {
                long i = 0;
                long.TryParse(data.SafeGet("DSpeed"), out i);
                return i;
            }
            set
            {
                data.Set("DSpeed", ((int)value).ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("DownloadSpeed");
            }
        }

        [JsonIgnoreAttribute]
        public long UploadSpeed
        {
            get
            {
                long i = 0;
                long.TryParse(data.SafeGet("USpeed"), out i);
                return i;
            }
            set
            {
                data.Set("USpeed", ((int)value).ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("UploadSpeed");
            }
        }

        [JsonIgnoreAttribute]
        public string Location
        {
            get { return data.SafeGet("Host") + ":" + data.SafeGet("Port"); }
            set
            {
                int index = value.IndexOf(':');
                Host = value.Substring(0, index);
                Port = int.Parse(value.Substring(index + 1, value.Length - (index + 1)));
                NotifyChange("Host");
                NotifyChange("Location");
            }
        }

        [JsonIgnoreAttribute]
        public string Host
        {
            get { return data.SafeGet("Host"); }
            set
            {
                data.Set("Host", value);
                LastUpdate = Environment.TickCount;
                NotifyChange("Host");
                NotifyChange("Location");
            }
        }

        [JsonIgnoreAttribute]
        public int Port
        {
            get
            {
                int i = 0;
                int.TryParse(data.SafeGet("Port"), out i);
                return i;
            }
            set
            {
                data.Set("Port", value.ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("Port");
                NotifyChange("Location");
            }
        }

        [JsonIgnoreAttribute]
        public string Nickname
        {
            get { return data.SafeGet("Nickname"); }
            set
            {
                data.Set("Nickname", value);
                LastUpdate = Environment.TickCount;
                NotifyChange("Nickname");
            }
        }

        [JsonIgnoreAttribute]
        public string Description
        {
            get { return data.SafeGet("Description"); }
            set
            {
                data.Set("Description", value);
                LastUpdate = Environment.TickCount;
                NotifyChange("Description");
            }
        }

        [JsonIgnoreAttribute]
        public long ShareSize
        {
            get
            {
                long i = 0;
                long.TryParse(data.SafeGet("ShareSize"), out i);
                return i;
            }
            set
            {
                data.Set("ShareSize", value.ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("ShareSize");
            }
        }

        [JsonIgnoreAttribute]
        public long FileCount
        {
            get
            {
                long i = 0;
                long.TryParse(data.SafeGet("FileCount"), out i);
                return i;
            }
            set
            {
                data.Set("FileCount", value.ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("FileCount");
            }
        }

        [JsonIgnoreAttribute]
        public string Avatar
        {
            get { return data.SafeGet("Avatar"); }
            set
            {
                data.Set("Avatar", value);
                LastUpdate = Environment.TickCount;
                NotifyChange("Avatar");
            }
        }

        public bool IsKeySet(string key)
        {
            return data.ContainsKey(key);
        }

        [DataMember(Name="Node")]
        public SafeDictionary<string, string> Data
        {
            get
            {
                return data;

            }
            set { data = value; }
        }


        public string GetData(string key)
        {
            return data.SafeGet(key);
        }

        public bool ContainsKey(string key)
        {
            return data.ContainsKey(key);
        }

        public void SetData(string key, string data)
        {
            this.data.Set(key, data);
            NotifyChange(key);

            switch (key)
            {
                case "Online":
                case "ClientVersion":
                case "ID":
                case "Speed":
                case "Nickname":
                case "Description":
                case "ShareSize":
                case "FileCount":
                case "Avatar":
                    NotifyChange(key);
                    break;
                case "Host":
                case "Port":
                case "Location":
                    NotifyChange("Host");
                    NotifyChange("Port");
                    NotifyChange("Location");
                    break;
            }
        }
    }
}
