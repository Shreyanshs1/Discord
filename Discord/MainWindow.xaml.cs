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

namespace Discord
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Get the text from the message input box.
            //    We can access UI elements from the C# code by using the 'x:Name'
            //    we gave them in the XAML file (e.g., MessageTextBox).
            string message = MessageTextBox.Text;

            // 2. Check if the message is not empty or just whitespace.
            //    We don't want to send empty messages.
            if (!string.IsNullOrWhiteSpace(message))
            {
                // 3. Add the message to the chat messages display.
                //    We'll add "You: " to the front to show who sent it.
                MessagesListBox.Items.Add($"You: {message}");

                // 4. Clear the message input box for the next message.
                MessageTextBox.Clear();

                // 5. (Optional but good practice) Set the keyboard focus back to the input box.
                MessageTextBox.Focus();
            }
        }

        private void UserListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}