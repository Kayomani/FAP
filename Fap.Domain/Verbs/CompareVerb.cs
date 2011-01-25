using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fap.Network;
using Fap.Network.Entity;
using Fap.Foundation;
using Fap.Domain.Entity;

namespace Fap.Domain.Verbs
{
    public class CompareVerb : VerbBase, IVerb
    {
        private Model model;
        private static Response cachedResponse = null;
        private static long cacheTime = 0;
        private object sync = new object();

        public CompareVerb(Model m)
        {
            model = m;
        }


        public Request CreateRequest()
        {
            Request r = new Request();
            r.Command = "COMPARE";
            return r;
        }

        public Network.Entity.Response ProcessRequest(Network.Entity.Request r)
        {
            if (model.DisableComparision)
            {
                Response response = new Response();
                response.RequestID = r.RequestID;
                response.Status = 10;
                return response;
            }

            lock (sync)
            {
                if (null == cachedResponse || Environment.TickCount - cacheTime > 1000 * 300)
                {
                    Response response = new Response();
                    SystemInfo si = new SystemInfo();
                    response.Status = 0;
                    response.RequestID = r.RequestID;
                    response.AdditionalHeaders.Add("COMP-CPUSpeed", si.GetCPUSpeed().ToString());
                    response.AdditionalHeaders.Add("COMP-CPUType", si.GetCPUType());
                    response.AdditionalHeaders.Add("COMP-CPUCores", si.GetCPUCores().ToString());
                    response.AdditionalHeaders.Add("COMP-CPUThreads", si.GetCPUThreads().ToString());
                    response.AdditionalHeaders.Add("COMP-CPUBits", si.GetCPUBits().ToString());
                    response.AdditionalHeaders.Add("COMP-MoboBrand", si.GetMoboBrand().ToString());
                    response.AdditionalHeaders.Add("COMP-MoboModel", si.GetMoboModel().ToString());
                    response.AdditionalHeaders.Add("COMP-BIOSVersion", si.GetBIOSVersion().ToString());
                    response.AdditionalHeaders.Add("COMP-RAMSize", si.GetMemorySize().ToString());
                    response.AdditionalHeaders.Add("COMP-GPUModel", si.GetGPUDescription().ToString());
                    response.AdditionalHeaders.Add("COMP-GPUCount", si.GetGPUCount().ToString());
                    response.AdditionalHeaders.Add("COMP-GPUTotalMemory", si.GetTotalGPUMemory().ToString());
                    response.AdditionalHeaders.Add("COMP-DisplayPrimaryHeight", si.GetPrimaryDisplayHeight().ToString());
                    response.AdditionalHeaders.Add("COMP-DisplayPrimaryWidth", si.GetPrimaryDisplayWidth().ToString());
                    response.AdditionalHeaders.Add("COMP-DisplayTotalWidth", si.GetTotalDisplayWidth().ToString());
                    response.AdditionalHeaders.Add("COMP-DisplayTotalHeight", si.GetTotalDisplayHeight().ToString());
                    response.AdditionalHeaders.Add("COMP-HDDSize", si.GetTotalHDDSize().ToString());
                    response.AdditionalHeaders.Add("COMP-HDDFree", si.GetTotalHDDFree().ToString());
                    response.AdditionalHeaders.Add("COMP-HDDCount", si.GetHDDCount().ToString());
                    response.AdditionalHeaders.Add("COMP-NICSpeed", si.GetNetworkSpeed().ToString());
                    response.AdditionalHeaders.Add("COMP-SoundCard", si.GetSoundcardName().ToString());
                    cachedResponse = response;
                    cacheTime = Environment.TickCount;
                }
            }
            return cachedResponse;
        }

        public bool ReceiveResponse(Network.Entity.Response r)
        {
            Node = new CompareNode();
            Status = r.Status;
            if (r.Status != 0)
                return false;

            foreach(var header in r.AdditionalHeaders)
                Node.SetData(header.Key, header.Value);
            Node.Score = Node.GetSystemScore();
            return true;
        }

        public CompareNode Node { set; get; }
    }
}
