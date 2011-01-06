using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using LinqToWmi.Core.WMI;
using System.Linq.Expressions;
using WmiEntities;
using System.IO;
using System.Management;
using System.Threading;

namespace LinqToWmi.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            using (WmiContext context = new WmiContext(@"\\."))
            {
                context.ManagementScope.Options.Impersonation = ImpersonationLevel.Impersonate;

                context.Log = Console.Out;

                QueryEvents(context);
                QueryProcessors(context);
                QueryDrives(context);

                QueryAvailableMemory(context);

                QueryAccounts(context);
            }
            
            Thread.Sleep(3000);
        }

        private static void QueryAccounts(WmiContext context)
        {
            var query = from account in context.Source<Win32_UserAccount>()
                        select account;

            foreach (Win32_UserAccount account in query)
            {
                Console.WriteLine(account.Name);
            }
        }

        /// <summary>
        /// Queries the available system memory
        /// </summary>
        /// <param name="context"></param>
        private static void QueryAvailableMemory(WmiContext context)
        {
            var query = from memory in context.Source<Win32_PerfFormattedData_PerfOS_Memory>()
                        select memory;

            Console.WriteLine(String.Format("Available memory: {0}MB.", query.Single().AvailableMBytes));

            ulong availMemory = (from computerSystem in context.Source<Win32_ComputerSystem>()
                                 select computerSystem.TotalPhysicalMemory / 1048576).Single();

            Console.WriteLine(String.Format("Available memory: {0}MB.", availMemory));
        }
        
        /// <summary>
        /// Example of querying CPUs
        /// </summary>
        /// <param name="context">The WMI query context</param>
        private static void QueryProcessors(WmiContext context)
        {
            var query = from processor in context.Source<Win32_Processor>()
                        select processor;

            foreach (Win32_Processor processor in query)
            {
                Console.WriteLine(String.Format("DeviceID: {0}", processor.DeviceID));
                Console.WriteLine(String.Format("Usage: {0}%", processor.LoadPercentage));
	        }

        }

        /// <summary>
        /// Example of querying the eventlog 'Application' and take 5
        /// </summary>
        /// <param name="context"></param>
        public static void QueryEvents(WmiContext context)
        {
            //NOTE: We havent overloaded 'Take' so it deferres to the not optimized IEnumerable<T> of Take
            var query = (from evt in context.Source<Win32_NTLogEvent>()
                         where
                             evt.Logfile == "Application" && evt.Message.Contains(".exe")
                         select evt).Take(5);

            foreach (Win32_NTLogEvent evt in query)
            {
                Console.WriteLine(evt.TimeWritten);
                Console.WriteLine(evt.Message);
            }
        }

        /// <summary>
        /// Example of querying system drives
        /// Query drives that do not have an X in their name, and more that 1024 bytes of space
        /// </summary>
        public static void QueryDrives(WmiContext context)
        {
            var query = from drive in context.Source<Win32_LogicalDisk>()
                        where drive.DriveType == 3 || drive.DriveType == 4
                        select drive;

            foreach (Win32_LogicalDisk drive in query)
            {
                Console.WriteLine(String.Format("Drive {0} has {1}MB free. Total size: {2}MB.", drive.DeviceID, 
                                  drive.FreeSpace / 106496,
                                  drive.Size / 106496,
                                  drive.DriveType));
            }
        }
    }
}