using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fap.Network.Entity;

namespace Fap.Domain.Entity
{
    public class CompareNode : Node
    {
        private long ParseString(string s)
        {
            long d = 0;
            long.TryParse(s, out d);
            return d;
        }

        public long GetSystemScore()
        {
            long total = 0;
            total += CPUSpeed * CPUThreads;
            total += CPUBits * 50;
            total += (long)((RAMSize / 1000000) * 1.25);
            total += (GPUCount * 1000);
            total += GPUTotalMemory / 2000000;
            total += ((DisplayTotalWidth * DisplayTotalHeight) / 10000);
            total += (long)((HDDFree / 1000000000) * 0.75);
            total += ((HDDSize - HDDFree) / 1000000000);
            return total;
        }


        public long CPUSpeed
        {
            get { return ParseString(data.SafeGet("COMP-CPUSpeed")); }
            set
            {
                data.Set("COMP-CPUSpeed", value.ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("CPUSpeed");
            }
        }

        public string CPUType
        {
            get { return data.SafeGet("COMP-CPUType"); }
            set
            {
                data.Set("COMP-CPUType", value);
                LastUpdate = Environment.TickCount;
                NotifyChange("CPUType");
            }
        }

        public long CPUCores
        {
            get { return ParseString(data.SafeGet("COMP-CPUCores")); }
            set
            {
                data.Set("COMP-CPUCores", value.ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("CPUCores");
            }
        }

        public long CPUThreads
        {
            get { return ParseString(data.SafeGet("COMP-CPUThreads")); }
            set
            {
                data.Set("COMP-CPUThreads", value.ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("CPUThreads");
            }
        }

        public long CPUBits
        {
            get { return ParseString(data.SafeGet("COMP-CPUBits")); }
            set
            {
                data.Set("COMP-CPUBits", value.ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("CPUBits");
            }
        }

        public string MoboBrand
        {
            get { return data.SafeGet("COMP-MoboBrand"); }
            set
            {
                data.Set("COMP-MoboBrand", value);
                LastUpdate = Environment.TickCount;
                NotifyChange("MoboBrand");
            }
        }

        public string MoboModel
        {
            get { return data.SafeGet("COMP-MoboModel"); }
            set
            {
                data.Set("COMP-MoboModel", value);
                LastUpdate = Environment.TickCount;
                NotifyChange("MoboModel");
            }
        }

        public string BIOSVersion
        {
            get { return data.SafeGet("COMP-BIOSVersion"); }
            set
            {
                data.Set("COMP-BIOSVersion", value);
                LastUpdate = Environment.TickCount;
                NotifyChange("BIOSVersion");
            }
        }

        public long RAMSize
        {
            get { return ParseString(data.SafeGet("COMP-RAMSize")); }
            set
            {
                data.Set("COMP-RAMSize", value.ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("RAMSize");
            }
        }

        public string GPUModel
        {
            get { return data.SafeGet("COMP-GPUModel"); }
            set
            {
                data.Set("COMP-GPUModel", value);
                LastUpdate = Environment.TickCount;
                NotifyChange("GPUModel");
            }
        }

        public long GPUCount
        {
            get { return ParseString(data.SafeGet("COMP-GPUCount")); }
            set
            {
                data.Set("COMP-GPUCount", value.ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("GPUCount");
            }
        }

        public long GPUTotalMemory
        {
            get { return ParseString(data.SafeGet("COMP-GPUTotalMemory")); }
            set
            {
                data.Set("COMP-GPUTotalMemory", value.ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("GPUTotalMemory");
            }
        }

        public long DisplayPrimaryHeight
        {
            get { return ParseString(data.SafeGet("COMP-DisplayPrimaryHeight")); }
            set
            {
                data.Set("COMP-DisplayPrimaryHeight", value.ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("DisplayPrimaryHeight");
            }
        }

        public long DisplayPrimaryWidth
        {
            get { return ParseString(data.SafeGet("COMP-DisplayPrimaryWidth")); }
            set
            {
                data.Set("COMP-DisplayPrimaryWidth", value.ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("DisplayPrimaryWidth");
            }
        }

        public long DisplayTotalWidth
        {
            get { return ParseString(data.SafeGet("COMP-DisplayTotalWidth")); }
            set
            {
                data.Set("COMP-DisplayTotalWidth", value.ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("DisplayTotalWidth");
            }
        }

        public long DisplayTotalHeight
        {
            get { return ParseString(data.SafeGet("COMP-DisplayTotalHeight")); }
            set
            {
                data.Set("COMP-DisplayTotalHeight", value.ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("DisplayTotalHeight");
            }
        }

        public long HDDSize
        {
            get { return ParseString(data.SafeGet("COMP-HDDSize")); }
            set
            {
                data.Set("COMP-HDDSize", value.ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("HDDSize");
            }
        }

        public long HDDFree
        {
            get { return ParseString(data.SafeGet("COMP-HDDFree")); }
            set
            {
                data.Set("COMP-HDDFree", value.ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("HDDFree");
            }
        }

        public long HDDCount
        {
            get { return ParseString(data.SafeGet("COMP-HDDCount")); }
            set
            {
                data.Set("COMP-HDDCount", value.ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("HDDCount");
            }
        }

        public long NICSpeed
        {
            get { return ParseString(data.SafeGet("COMP-NICSpeed")); }
            set
            {
                data.Set("COMP-NICSpeed", value.ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("NICSpeed");
            }
        }

        public string SoundCard
        {
            get { return data.SafeGet("COMP-SoundCard"); }
            set
            {
                data.Set("COMP-SoundCard", value);
                LastUpdate = Environment.TickCount;
                NotifyChange("SoundCard");
            }
        }

        public long Score
        {
            get { return ParseString(data.SafeGet("COMP-Score")); }
            set
            {
                data.Set("COMP-Score", value.ToString());
                LastUpdate = Environment.TickCount;
                NotifyChange("Score");
            }
        }
    }
}
