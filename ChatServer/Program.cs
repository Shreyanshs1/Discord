using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatServer
{
    class Program
    {
        static readonly Dictionary<TcpClient, string> _clients = new Dictionary<TcpClient, string>();
        private static readonly object _lock = new object();

        static void Main(string[] args)
        {
            int port = 8888;
            TcpListener server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Console.WriteLine($"Server started on port {port}. Waiting for connections...");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();

                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
        }

        static void HandleClient(TcpClient tcpClient)
        {
            string username = "[Connecting]";
            try
            {
                NetworkStream stream = tcpClient.GetStream();
                byte[] buffer = new byte[1024];
                int byteCount;

                // 1. Read the username first.
                // The first message from the client MUST be their desired username.
                byteCount = stream.Read(buffer, 0, buffer.Length);
                username = Encoding.UTF8.GetString(buffer, 0, byteCount);

                // Add the new client and their username to the dictionary.
                lock (_lock)
                {
                    _clients.Add(tcpClient, username);
                }

                Console.WriteLine($"{username} connected!");
                BroadcastMessage($"{username} has joined the chat.", tcpClient, isSystemMessage: true);
                BroadcastUserList(); // Send the updated user list to everyone.

                // 2. Loop to read chat messages.
                while ((byteCount = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, byteCount);

                    // *** NEW LOGIC IS HERE ***
                    if (message.StartsWith("§PAINT§"))
                    {
                        // It's a paint message. Broadcast it to everyone else.
                        BroadcastMessage(message, tcpClient);
                    }
                    else
                    {
                        BroadcastMessage($"{username}: {message}", tcpClient);
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"{username} has disconnected.");
            }
            finally
            {
                // 3. Clean up on disconnect.
                lock (_lock)
                {
                    _clients.Remove(tcpClient);
                }
                tcpClient.Close();
                BroadcastMessage($"{username} has left the chat.", tcpClient, isSystemMessage: true);
                BroadcastUserList(); // Send the updated user list to everyone.
            }
        }

        static void BroadcastMessage(string message, TcpClient excludeClient, bool isSystemMessage = false)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);

            lock (_lock)
            {
                foreach (var client in _clients.Keys)
                {
                    // Don't send chat messages back to the sender
                    if (client != excludeClient || isSystemMessage)
                    {
                        NetworkStream stream = client.GetStream();
                        stream.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }

        static void BroadcastUserList()
        {
            // We'll create a special message to send the user list.
            // Let's format it like: §USERLIST§user1,user2,user3
            lock (_lock)
            {
                string userListMessage = "§USERLIST§" + string.Join(",", _clients.Values);
                byte[] buffer = Encoding.UTF8.GetBytes(userListMessage);

                foreach (var client in _clients.Keys)
                {
                    NetworkStream stream = client.GetStream();
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}