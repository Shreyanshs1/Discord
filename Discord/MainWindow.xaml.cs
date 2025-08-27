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
using System;
using System.Net.Sockets;
using System.Threading;

namespace Discord
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient? _client;
        private NetworkStream? _stream;
        private Thread? _listenThread;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectToServer(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Create a new TcpClient and connect to the server.
                _client = new TcpClient("127.0.0.1", 8888);
                _stream = _client.GetStream();

                // 2. Start a new thread to listen for messages from the server.
                // This is crucial to prevent the UI from freezing.
                _listenThread = new Thread(ListenForMessages);
                _listenThread.IsBackground = true; // Ensures the thread closes when the app closes.
                _listenThread.Start();

                MessagesListBox.Items.Add("Connected to server!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to server: {ex.Message}");
            }
        }

        private void ListenForMessages()
        {
            byte[] buffer = new byte[1024];
            int byteCount;

            try
            {
                // 3. Loop indefinitely to read messages from the server.
                while ((byteCount = _stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string message = Encoding.ASCII.GetString(buffer, 0, byteCount);

                    // 4. IMPORTANT: Update the UI from the UI thread.
                    // A background thread cannot directly change the UI.
                    // We must use Dispatcher.Invoke to safely update the ListBox.
                    Dispatcher.Invoke(() =>
                    {
                        MessagesListBox.Items.Add(message);
                    });
                }
            }
            catch (Exception)
            {
                // Handle disconnection or other errors.
                Dispatcher.Invoke(() =>
                {
                    MessagesListBox.Items.Add("Server disconnected.");
                });
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text;
            if (!string.IsNullOrWhiteSpace(message) && _stream != null)
            {
                MessagesListBox.Items.Add($"You: {message}");

                byte[] buffer = Encoding.ASCII.GetBytes($"A client says: {message}");
                _stream.Write(buffer, 0, buffer.Length);

                MessageTextBox.Clear();
                MessageTextBox.Focus();
            }
        }


        private void UserListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}