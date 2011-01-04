using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Waf.Applications.Services;
using System.ComponentModel.Composition;

namespace Writer.Applications.Test.Services
{
    [Export(typeof(IMessageService))]
    public class MessageServiceMock : IMessageService
    {
        public MessageType MessageType { get; private set; }
        public string Message { get; private set; }

        
        public void ShowMessage(string message)
        {
            MessageType = MessageType.Message;
            Message = message;
        }

        public void ShowWarning(string message)
        {
            MessageType = MessageType.Warning;
            Message = message;
        }

        public void ShowError(string message)
        {
            MessageType = MessageType.Error;
            Message = message;
        }
    }

    public enum MessageType
    {
        Message,
        Warning,
        Error
    }
}
