using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace Discord // Your namespace might be different
{
    public partial class MainWindow : Window
    {
        private TcpClient? _client;
        private NetworkStream? _stream;
        private Thread? _listenThread;
        private string? _username;
        private readonly ChatView _chatView;
        private readonly CanvasView _canvasView;

        public MainWindow()
        {
            InitializeComponent();
            
            //initialize different views
            _chatView = new ChatView();
            _canvasView = new CanvasView();

            //set default view to chatView
            MainContentArea.Content = _chatView;

            //Listen to events from different views
            _chatView.MessageSent += HandleMessageSentFromChatView;
            _canvasView.OnDraw += SendPaintDataToServer;

        }

        // This method is now an event handler for your "Connect" button.
        private void ConnectToServer(object sender, RoutedEventArgs e)
        {
            // 1. Validate username.
            _username = UserName.Text;
            if (string.IsNullOrWhiteSpace(_username))
            {
                MessageBox.Show("Please enter a username.");
                return;
            }

            try
            {
                _client = new TcpClient("127.0.0.1", 8888);
                _stream = _client.GetStream();

                // 2. Send the username to the server immediately after connecting.
                byte[] usernameBytes = Encoding.UTF8.GetBytes(_username);
                _stream.Write(usernameBytes, 0, usernameBytes.Length);

                // Start listening for messages from the server.
                _listenThread = new Thread(ListenForMessages);
                _listenThread.IsBackground = true;
                _listenThread.Start();

                _chatView.AddMessage("Connected to server!");

                // 3. Update the UI.
                ConnectButton.IsEnabled = false;
                UserName.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to server: {ex.Message}");
            }
        }

        private void HandleMessageSentFromChatView(string message)
        {
            if (!string.IsNullOrWhiteSpace(message) && _stream != null && _client.Connected)
            {
                _chatView.AddMessage($"You: {message}");
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                _stream.Write(buffer, 0, buffer.Length);
            }
        }

        private void SendPaintDataToServer(string paintData)
        {
            if (_stream != null && _client.Connected)
            {
                try
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(paintData);
                    _stream.Write(buffer, 0, buffer.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending paint data: {ex.Message}");
                }
            }
        }

        private void ListenForMessages()
        {
            byte[] buffer = new byte[1024];
            int byteCount;

            try
            {
                while ((byteCount = _stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, byteCount);

                    // 4. Check if the message is a user list update.
                    if (message.StartsWith("§USERLIST§"))
                    {
                        // Update the user list box.
                        string[] users = message.Substring("§USERLIST§".Length).Split(',');
                        Dispatcher.Invoke(() =>
                        {
                            UserListBox.Items.Clear();
                            foreach (var user in users)
                            {
                                UserListBox.Items.Add(user);
                            }
                        });
                    }
                    else if (message.StartsWith("§PAINT§"))
                    {
                        // It's paint data! Update the canvas.
                        Dispatcher.Invoke(() =>
                        {
                            _canvasView.DrawLineFromServer(message);
                        });
                    }
                    else
                    {
                        // It's a regular chat message.
                        Dispatcher.Invoke(() =>
                        {
                            _chatView.AddMessage(message);
                        });
                    }
                }
            }
            catch (Exception)
            {
                Dispatcher.Invoke(() => _chatView.AddMessage("Server disconnected."));
            }
        }

        private void ShowChatView_Click(object sender, RoutedEventArgs e)
        {
            // Set the Content of the ContentControl to our ChatView instance
            MainContentArea.Content = _chatView;
        }

        private void ShowCanvasView_Click(object sender, RoutedEventArgs e)
        {
            // Set the Content of the ContentControl to our CanvasView instance
            MainContentArea.Content = _canvasView;
        }
        // Make sure you have this Closing event in your MainWindow.xaml
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _stream?.Close();
            _client?.Close();
        }
    }
}