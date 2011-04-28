using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FAP.Application.Views;
using FAP.Application.ViewModels;

namespace Fap.Presentation.Panels
{
    /// <summary>
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class Conversation : UserControl, IConverstationView
    {
        public Conversation()
        {
            InitializeComponent();
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(Conversation_DataContextChanged);
        }

        private ConversationViewModel Model
        {
            get { return DataContext as ConversationViewModel; }
        }

        void Conversation_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (null != Model)
            {
                sendText.CommandParameter = Model;
                sendText.Command = Model.SendChatMessage;
            }
        }

        private void inputText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.IsDown)
            {
                if (null != Model)
                {
                    Model.CurrentChatMessage = chatTextBox.Text;
                    Model.SendChatMessage.Execute(Model);
                }
            }
        }
    }
}
