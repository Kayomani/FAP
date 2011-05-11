/*  Minor adaptation from : http://customurl.codeplex.com
 * Licence: GNU General Public License version 2 (GPLv2)
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Fap.Foundation.RegistryServices
{
    public class RegisterProtocolService
    {

        public bool Register(string protocol, string application, string arguments)
        {
            return Register(protocol, application, arguments, Registry.LocalMachine);
        }

        public bool Register(string protocol, string application, string arguments, RegistryKey registry)
        {

            RegistryKey cl = Registry.ClassesRoot.OpenSubKey(protocol);

          //  if (cl != null && cl.GetValue("URL Protocol") != null && cl.GetValue("CustomUrlApplication") != null)
           // {
           //     return true;
           // }

            
            try
            {
                RegistryKey r;
                r = registry.OpenSubKey("SOFTWARE\\Classes\\" + protocol, true);
                if (r == null)
                    r = registry.CreateSubKey("SOFTWARE\\Classes\\" + protocol);
                r.SetValue("URL Protocol", "");
                r.SetValue("CustomUrlApplication", application);
                r.SetValue("CustomUrlArguments", arguments);

                r = registry.OpenSubKey("SOFTWARE\\Classes\\" + protocol + "\\DefaultIcon", true);
                if (r == null)
                    r = registry.CreateSubKey("SOFTWARE\\Classes\\" + protocol + "\\DefaultIcon");
                r.SetValue("", application);

                r = registry.OpenSubKey("SOFTWARE\\Classes\\" + protocol + "\\shell\\open\\command", true);
                if (r == null)
                    r = registry.CreateSubKey("SOFTWARE\\Classes\\" + protocol + "\\shell\\open\\command");

                r.SetValue("", application + " " + arguments);


                // If 64-bit OS, also register in the 32-bit registry area. 
                if (registry.OpenSubKey("SOFTWARE\\Wow6432Node\\Classes") != null)
                {
                    r = registry.OpenSubKey("SOFTWARE\\Wow6432Node\\Classes\\" + protocol, true);
                    if (r == null)
                        r = registry.CreateSubKey("SOFTWARE\\Wow6432Node\\Classes\\" + protocol);
                    r.SetValue("URL Protocol", "");
                    r.SetValue("CustomUrlApplication", application);
                    r.SetValue("CustomUrlArguments", arguments);

                    r = registry.OpenSubKey("SOFTWARE\\Wow6432Node\\Classes\\" + protocol + "\\DefaultIcon", true);
                    if (r == null)
                        r = registry.CreateSubKey("SOFTWARE\\Wow6432Node\\Classes\\" + protocol + "\\DefaultIcon");
                    r.SetValue("", application);

                    r = registry.OpenSubKey("SOFTWARE\\Wow6432Node\\Classes\\" + protocol + "\\shell\\open\\command", true);
                    if (r == null)
                        r = registry.CreateSubKey("SOFTWARE\\Wow6432Node\\Classes\\" + protocol + "\\shell\\open\\command");

                    r.SetValue("", application + " " + arguments);

                }
                r.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool Unregister(string protocol)
        {
            try
            {
                if (Registry.CurrentUser.OpenSubKey("Software\\Classes\\" + protocol) != null)
                    Registry.CurrentUser.OpenSubKey("Software\\Classes", true).DeleteSubKeyTree(protocol);
                if (Registry.CurrentUser.OpenSubKey("Software\\Wow6432Node\\Classes\\" + protocol) != null)
                    Registry.CurrentUser.OpenSubKey("Software\\Wow6432Node\\Classes", true).DeleteSubKeyTree(protocol);
                if (Registry.LocalMachine.OpenSubKey("Software\\Classes\\" + protocol) != null)
                    Registry.LocalMachine.OpenSubKey("Software\\Classes", true).DeleteSubKeyTree(protocol);
                if (Registry.LocalMachine.OpenSubKey("Software\\Wow6432Node\\Classes\\" + protocol) != null)
                    Registry.LocalMachine.OpenSubKey("Software\\Wow6432Node\\Classes", true).DeleteSubKeyTree(protocol);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
