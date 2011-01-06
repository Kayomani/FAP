using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToWmi.ProtoGenerator
{
    class Arguments
    {
        private string _wmi;
        private string _out;
        private string _ns = "WmiEntities";
        private string _provider = "CSharp";
        
        [Argument("wmi", Required = true, HelpText = "WMI object to create class for!")]
        public string Wmi
        {
            get { return _wmi; }
            set { _wmi = value; }
        }

        [Argument("out", HelpText  = "Filename to create")]
        public string Out
        {
            get { return _out; }
            set { _out = value; }
        }

        [Argument("ns", HelpText = "Namespace")]
        public string Ns
        {
            get { return _ns; }
            set { _ns = value; }
        }

        [Argument("provider", HelpText = "Language to generate the file for (IE. CSharp)")]
        public string Provider
        {
            get { return _provider; }
            set { _provider = value; }
        }
    }
}
