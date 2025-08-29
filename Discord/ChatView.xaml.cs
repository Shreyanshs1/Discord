using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Discord
{
    /// <summary>
    /// Interaction logic for ChatView.xaml
    /// </summary>
    public partial class ChatView : UserControl
    {
        public event Action<string> MessageSent;

        public ChatView()
        {
            InitializeComponent();
        }

        private void SendButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SendMessage();
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        private void SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(MessageTextBox.Text))
            {
                // Raise the event, passing the message text to the listener (MainWindow).
                MessageSent?.Invoke(MessageTextBox.Text);
                MessageTextBox.Clear();
            }
        }

        public void AddMessage(string message)
        {
            MessagesListBox.Items.Add(message);
            // Auto-scroll to the latest message
            MessagesListBox.ScrollIntoView(MessagesListBox.Items[MessagesListBox.Items.Count - 1]);
        }
    }
}
