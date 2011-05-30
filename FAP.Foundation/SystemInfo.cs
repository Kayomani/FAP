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
using Microsoft.Win32;
using LinqToWmi.Core.WMI;
using System.Net.NetworkInformation;
using Fap.Foundation.WMI_Prototypes;
using Fap.Foundation;

namespace Fap.Foundation
{
    /// <summary>
    /// Retrives system information.  Mostly uses linqToWMI which is licenced under LGPL.
    /// </summary>
    public class SystemInfo
    {
        public void Test()
        {
            Console.WriteLine("CPU speed: " + GetCPUSpeed()); //Win32_Processor
            Console.WriteLine("CPU type: " + GetCPUType());
            Console.WriteLine("CPU cores: " + GetCPUThreads());
            Console.WriteLine("CPU threads: " + GetCPUCores());
            Console.WriteLine("CPU bits: " + GetCPUBits());
            Console.WriteLine("Mobo Brand: " + GetMoboBrand()); //Win32_BaseBoard
            Console.WriteLine("Mobo Model: " + GetMoboModel());
            Console.WriteLine("BIOS: " + GetBIOSVersion()); //Win32_BIOS
            Console.WriteLine("RAM Size: " + GetMemorySize()); //Win32_PhysicalMemory
            Console.WriteLine("GPU Model: " + GetGPUDescription());//Win32_VideoController
            Console.WriteLine("GPU Count: " + GetGPUCount());
            Console.WriteLine("GPU total memory: " + GetTotalGPUMemory());
            Console.WriteLine("Primary display height: " + GetPrimaryDisplayHeight());//Win32_VideoController
            Console.WriteLine("Primary display width: " + GetPrimaryDisplayWidth());
            Console.WriteLine("Total display height: " + GetTotalDisplayHeight());
            Console.WriteLine("Total display width: " + GetTotalDisplayWidth());
            Console.WriteLine("Hard disk size: " + GetTotalHDDSize()); //Win32_LogicalDisk
            Console.WriteLine("Hard disk free: " + GetTotalHDDFree());
            Console.WriteLine("Hard disk count: " + GetHDDCount());
            Console.WriteLine("NIC Speed: " + GetNetworkSpeed()); //Win32_NetworkAdapter
            //Console.WriteLine("NIC Sent: " + GetNetworkSent());
            // Console.WriteLine("NIC Received: " + GetNetworkReceived()); 
            Console.WriteLine("Sound card: " + GetSoundcardName()); //Win32_SoundDevice
           // Console.WriteLine("System score: " + GetSystemScore());
            Console.ReadKey();
        }


       

        public int GetCPUSpeed()
        {

            string item = RegistryHelper.GetRegistryData(Registry.LocalMachine, "HARDWARE/DESCRIPTION/System/CentralProcessor/0/~MHz");
            int i = 0;
            int.TryParse(item, out i);
            return i;
        }

        public string GetCPUType()
        {
            return RegistryHelper.GetRegistryData(Registry.LocalMachine, "HARDWARE/DESCRIPTION/System/CentralProcessor/0/ProcessorNameString");
        }

        public int GetCPUCores()
        {
            try
            {
                using (WmiContext context = new WmiContext(@"\\localhost"))
                {
                    return (int)context.Source<Win32_Processor>().First().NumberOfCores;
                }
            }
            catch
            {
                return 1;
            }
        }

        public int GetCPUThreads()
        {
            try
            {
                using (WmiContext context = new WmiContext(@"\\localhost"))
                {
                    return (int)context.Source<Win32_Processor>().First().NumberOfLogicalProcessors;
                }
            }
            catch
            {
                return 1;
            }
        }

        public int GetCPUBits()
        {
            try
            {
                using (WmiContext context = new WmiContext(@"\\localhost"))
                    return (int)context.Source<Win32_Processor>().First().AddressWidth;
            }
            catch { return 1; }
        }

        public string GetMoboBrand()
        {
            try
            {
                using (WmiContext context = new WmiContext(@"\\localhost"))
                    return context.Source<Win32_BaseBoard>().First().Manufacturer;
            }
            catch { return string.Empty; }
        }

        public string GetMoboModel()
        {
            try
            {
                using (WmiContext context = new WmiContext(@"\\localhost"))
                    return context.Source<Win32_BaseBoard>().First().Product;
            }
            catch { return string.Empty; }
        }

        public string GetBIOSVersion()
        {
            try
            {
                using (WmiContext context = new WmiContext(@"\\localhost"))
                    return context.Source<Win32_BIOS>().First().SMBIOSBIOSVersion;
            }
            catch { return string.Empty; }
        }

        public long GetMemorySize()
        {
            try
            {
                using (WmiContext context = new WmiContext(@"\\localhost"))
                {
                    var totals = from stick in context.Source<Win32_PhysicalMemory>()
                                 select (long)stick.Capacity;
                    return totals.Sum();
                }
            }
            catch { return 0; }
        }

        public string GetGPUDescription()
        {
            try
            {
                using (WmiContext context = new WmiContext(@"\\localhost"))
                {
                    return context.Source<Win32_VideoController>().First().Name;
                }
            }
            catch { return string.Empty; }
        }

        public int GetGPUCount()
        {
            try
            {
                using (WmiContext context = new WmiContext(@"\\localhost"))
                {
                    return context.Source<Win32_VideoController>().Count();
                }
            }
            catch { return 1; }
        }


        public long GetTotalGPUMemory()
        {
            try
            {
                using (WmiContext context = new WmiContext(@"\\localhost"))
                {
                    var totals = from card in context.Source<Win32_VideoController>()
                                 select (long)card.AdapterRAM;
                    return totals.Sum();
                }
            }
            catch { return 1; }
        }
        public int GetPrimaryDisplayHeight()
        {
            try
            {
                return System.Windows.Forms.Screen.AllScreens.Where(s => s.Primary).FirstOrDefault().Bounds.Height;
            }
            catch { return 0; }
        }

        public int GetPrimaryDisplayWidth()
        {
            try
            {
                return System.Windows.Forms.Screen.AllScreens.Where(s => s.Primary).FirstOrDefault().Bounds.Width;
            }
            catch { return 0; }
        }

        public int GetTotalDisplayHeight()
        {
            try
            {
                var height = from s in System.Windows.Forms.Screen.AllScreens
                             select s.Bounds.Height;
                return height.Sum();
            }
            catch { return 0; }
        }

        public int GetTotalDisplayWidth()
        {
            try
            {
                var height = from s in System.Windows.Forms.Screen.AllScreens
                             select s.Bounds.Width;
                return height.Sum();
            }
            catch { return 0; }
        }

        /// <summary>
        /// Note display size is (value * 1073741824)/1000000000
        /// </summary>
        /// <returns></returns>
        public long GetTotalHDDSize()
        {
            try
            {
                using (WmiContext context = new WmiContext(@"\\localhost"))
                {
                    var totals = from disk in context.Source<Win32_LogicalDisk>()
                                 where disk.DriveType == 3
                                 select (long)disk.Size;
                    return totals.Sum();
                }
            }
            catch { return 0; }
        }

        public long GetTotalHDDFree()
        {
            try
            {
                using (WmiContext context = new WmiContext(@"\\localhost"))
                {
                    var totals = from disk in context.Source<Win32_LogicalDisk>()
                                 where disk.DriveType == 3
                                 select (long)disk.FreeSpace;
                    return totals.Sum();
                }
            }
            catch { return 0; }
        }

        public int GetHDDCount()
        {
            try
            {
                using (WmiContext context = new WmiContext(@"\\localhost"))
                    return context.Source<Win32_LogicalDisk>().Where(d => d.DriveType == 3).Count();
            }
            catch { return 0; }
        }


        public string GetSoundcardName()
        {
            try
            {
                using (WmiContext context = new WmiContext(@"\\localhost"))
                    return context.Source<Win32_SoundDevice>().First().Name;
            }
            catch { return string.Empty; }
        }


        public long GetNetworkSpeed()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var inter in interfaces)
            {
                if (inter.OperationalStatus == OperationalStatus.Up)
                    return inter.Speed;
            }
            return 0;
        }


        public long GetNetworkSent()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var inter in interfaces)
            {
                if (inter.OperationalStatus == OperationalStatus.Up)
                    return inter.GetIPv4Statistics().BytesSent;
            }
            return 0;
        }

        public long GetNetworkReceived()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var inter in interfaces)
            {
                if (inter.OperationalStatus == OperationalStatus.Up)
                {
                    var d = inter.GetIPv4Statistics();
                    return d.BytesReceived;
                }
            }
            return 0;
        }

        /*private string GetWMIInfo(string root, string query, string value)
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(root, query);
                foreach (ManagementObject obj in searcher.Get())
                {
                    return queryObj[value];
                }
            }
            catch (ManagementException e)
            {
                return string.Empty;
            }
        }*/
    }
}
