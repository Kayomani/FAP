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
using System.IO;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;

namespace FAP.Domain.Verbs
{
    public class BaseVerb
    {
        public static T Deserialise<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
            /*T obj = Activator.CreateInstance<T>();
            using (MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(json)))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
                obj = (T)serializer.ReadObject(ms); 
                return obj;
            }*/
        }
        
        public static string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            /* DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
             using (MemoryStream ms = new MemoryStream())
             {
                 serializer.WriteObject(ms, obj);
                 //return Encoding.ASCII.GetString(ms.ToArray());
             }*/
        }
    }
}
