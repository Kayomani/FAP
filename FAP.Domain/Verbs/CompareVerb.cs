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
using FAP.Network.Entities;
using FAP.Domain.Entities;
using Fap.Foundation;

namespace FAP.Domain.Verbs
{
    public class CompareVerb : BaseVerb, IVerb
    {
        private Model model;
        private static object sync = new object();
        private static NetworkRequest cachedResponse = null;
        private static long cacheTime = 0;

        public CompareVerb(Model m)
        {
            model = m;
        }


        public NetworkRequest CreateRequest()
        {
            NetworkRequest req = new NetworkRequest();
            req.Verb = "COMPARE";
            return req;
        }

        public NetworkRequest ProcessRequest(Network.Entities.NetworkRequest r)
        {
             Allowed = !model.DisableComparision;
             if (Allowed)
             {
                 lock (sync)
                 {
                     if (null == cachedResponse || Environment.TickCount - cacheTime > 1000 * 300)
                     {

                         SystemInfo si = new SystemInfo();
                         Node = new CompareNode();
                         Node.SetData("COMP-CPUSpeed", si.GetCPUSpeed().ToString());
                         Node.SetData("COMP-CPUType", si.GetCPUType());
                         Node.SetData("COMP-CPUCores", si.GetCPUCores().ToString());
                         Node.SetData("COMP-CPUThreads", si.GetCPUThreads().ToString());
                         Node.SetData("COMP-CPUBits", si.GetCPUBits().ToString());
                         Node.SetData("COMP-MoboBrand", si.GetMoboBrand().ToString());
                         Node.SetData("COMP-MoboModel", si.GetMoboModel().ToString());
                         Node.SetData("COMP-BIOSVersion", si.GetBIOSVersion().ToString());
                         Node.SetData("COMP-RAMSize", si.GetMemorySize().ToString());
                         Node.SetData("COMP-GPUModel", si.GetGPUDescription().ToString());
                         Node.SetData("COMP-GPUCount", si.GetGPUCount().ToString());
                         Node.SetData("COMP-GPUTotalMemory", si.GetTotalGPUMemory().ToString());
                         Node.SetData("COMP-DisplayPrimaryHeight", si.GetPrimaryDisplayHeight().ToString());
                         Node.SetData("COMP-DisplayPrimaryWidth", si.GetPrimaryDisplayWidth().ToString());
                         Node.SetData("COMP-DisplayTotalWidth", si.GetTotalDisplayWidth().ToString());
                         Node.SetData("COMP-DisplayTotalHeight", si.GetTotalDisplayHeight().ToString());
                         Node.SetData("COMP-HDDSize", si.GetTotalHDDSize().ToString());
                         Node.SetData("COMP-HDDFree", si.GetTotalHDDFree().ToString());
                         Node.SetData("COMP-HDDCount", si.GetHDDCount().ToString());
                         Node.SetData("COMP-NICSpeed", si.GetNetworkSpeed().ToString());
                         Node.SetData("COMP-SoundCard", si.GetSoundcardName().ToString());
                         cachedResponse = new NetworkRequest() { Data = Serialize<CompareVerb>(this) };
                         cacheTime = Environment.TickCount;
                     }
                 }
                 return cachedResponse;
             }
             else
             {
                 return new NetworkRequest() { Data = Serialize<CompareVerb>(this) };
             }
        }

        public bool ReceiveResponse(Network.Entities.NetworkRequest r)
        {
            try
            {
                CompareVerb inc = Deserialise<CompareVerb>(r.Data);
                Node = inc.Node;
                Allowed = inc.Allowed;
                if (null != Node)
                    Node.Score = inc.Node.GetSystemScore();
                else
                    Node = new CompareNode();
                return true;
            }
            catch { return false; }
        }

        public CompareNode Node { set; get; }
        public bool Allowed { set; get; }
    }
}
