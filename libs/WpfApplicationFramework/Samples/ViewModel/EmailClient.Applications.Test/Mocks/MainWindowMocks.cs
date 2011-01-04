using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmailClient.Applications.Views;
using EmailClient.Applications.ViewModels;

namespace EmailClient.Applications.Test.Mocks
{
    public class ShellViewMock : ViewMock<ShellViewModel>, IShellView
    {
        public bool IsVisible { get; private set; }

        public void Show()
        {
            IsVisible = true;
        }

        public void Close()
        {
            IsVisible = false;
        }
    }

    public class MessageListViewMock : ViewMock<MessageListViewModel>, IMessageListView 
    {
    }

    public class MessageContentViewMock : ViewMock<MessageContentViewModel>, IMessageContentView 
    {
    }
}
