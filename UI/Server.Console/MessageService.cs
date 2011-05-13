using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications.Services;

namespace Server.Console
{
    public class MessageService : IMessageService
    {

        public void ShowMessage(string message)
        {
            System.Console.WriteLine(message);
        }

        public void ShowWarning(string message)
        {
            System.Console.WriteLine("WARNING: " + message);
        }

        public void ShowError(string message)
        {
           System.Console.WriteLine("ERROR: " + message);
        }
    }
}
