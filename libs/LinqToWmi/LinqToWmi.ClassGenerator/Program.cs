using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Management;

namespace LinqToWmi.ProtoGenerator
{
    class Program
    {
        static void Main(string[] arguments)
        {
            Console.WriteLine("WMIClassGen - Class generator for WMI objects\n");

            Arguments commands = null;

            try
            {
                commands = ArgumentParser.Create<Arguments>(arguments);
            }
            catch (ArgumentParseException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            ManagementObject metaInfo = null;

            try
            {
                WmiMetaInformation query = new WmiMetaInformation();
                metaInfo = query.GetMetaInformation(commands.Wmi);
            }
            catch (ManagementException ex)
            {
                Console.WriteLine(String.Format("Error retrieving WMI information: {0}", ex.Message));
                return;
            }

            WmiFileGenerator generator = new WmiFileGenerator();
            generator.OutputLanguage = commands.Provider;
            generator.Namespace = commands.Ns;

            generator.GenerateFile(metaInfo, commands.Wmi, commands.Out);
            Console.WriteLine(String.Format("Generated file for object '{0}'", commands.Wmi));

        }
    }
}