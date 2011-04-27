using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToWmi.ProtoGenerator
{
    /// <summary>
    /// Exception that is thrown when invalid arguments are 
    /// </summary>
    class ArgumentParseException : ApplicationException
    {
        private ArgumentAttribute []  _arguments;

        public ArgumentParseException(ArgumentAttribute[] arguments) {
            _arguments = arguments;
        }

        public ArgumentAttribute [] Arguments
        {
            get { return _arguments; }
        }

        public override string Message
        {
            get
            {
                return String.Format("ERROR: Required parameter missing\n\n{0}", SwitchesHelpText);
            }
        }

        public string SwitchesHelpText
        {
            get { 
                string switches = "Switches:\n";
                foreach(ArgumentAttribute attr in Arguments) {
                    switches += String.Format(" /{0}- {1} {2}\n", 
                        attr.Cmd.PadRight(10), attr.HelpText, attr.Required ? "(Required)" :""); 
                }
                return switches;
            }
        }
	
    }
}
