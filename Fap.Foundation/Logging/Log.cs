using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fap.Foundation.Logging
{
    public class Log
    {
        public enum LogType { Info, Warning, Error};

        public LogType Type {set;get;}
        public string Message { set; get; }
        public DateTime When { set; get; }


        public string DisplayString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(When.ToShortTimeString());
                sb.Append(" ");
                sb.Append(Message);
                return sb.ToString();
            }
        }
    }
}
