using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToWmi.ProtoGenerator
{
    /// <summary>
    /// Indicates that an property can be used as an argument in an commandline application
    /// </summary>
    public class ArgumentAttribute : Attribute
    {
        private bool _required;
        private string _help;
        private string _cmd;

        public ArgumentAttribute(string cmd) {
            _cmd = cmd;
        }

        /// <summary>
        /// The user input command
        /// </summary>
        public string Cmd
        {
            get { return _cmd; }
        }

        /// <summary>
        /// Wheter the command is required as input
        /// </summary>  
        public bool Required
        {
            get { return _required; }
            set { _required = value; }
        }
	
        /// <summary>
        /// The helptext when an user inputs an invalid command
        /// </summary>		
        public string HelpText
        {
            get { return _help; }
            set { _help = value; }
        }
    }
}
