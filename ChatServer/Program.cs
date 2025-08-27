using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatServer
{
    class Program
    {
        // A list to hold all connected clients. We use TcpClient to represent a client connection.
        static readonly List<TcpClient> _clients = new List<TcpClient>();

        // A lock object for thread safety when accessing the _clients list.
        private static readonly object _lock = new object();

        static void Main(string[] args)
        {
            // 1. Define the IP address and port for the server to listen on.
            // IPAddress.Any means it will listen for connections on any available network interface.
            // 127.0.0.1 (localhost) would also work for local testing.
            int port = 8888;
            TcpListener server = new TcpListener(IPAddress.Any, port);

            try
            {
                // 2. Start the server.
                server.Start();
                Console.WriteLine($"Server started on port {port}. Waiting for connections...");

                // 3. Enter an infinite loop to continuously accept new clients.
                while (true)
                {
                    // Wait for a client to connect. This line blocks until a connection is made.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine($"Client connected! {client}");

                    // Add the new client to our list. The 'lock' ensures that
                    // only one thread can modify the list at a time.
                    lock (_lock)
                    {
                        _clients.Add(client);
                    }

                    // 4. Create a new thread to handle communication with this client.
                    // This allows the server to handle multiple clients at the same time.
                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                // Stop the server if the loop ever exits (e.g., due to an error).
                server.Stop();
            }
        }

        static void HandleClient(TcpClient tcpClient)
        {
            try
            {
                // Get the network stream for reading and writing data.
                NetworkStream stream = tcpClient.GetStream();
                byte[] buffer = new byte[1024]; // A buffer to store the received data.
                int byteCount;

                // Loop to continuously read data from the client.
                while ((byteCount = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Convert the received bytes into a string.
                    string message = Encoding.ASCII.GetString(buffer, 0, byteCount);
                    Console.WriteLine($"Received: {message}");

                    // Broadcast the message to all other clients.
                    BroadcastMessage(message, tcpClient);
                }
            }
            catch (Exception)
            {
                // An error occurred (e.g., client disconnected abruptly).
                Console.WriteLine("A client has disconnected.");
            }
            finally
            {
                // When the loop breaks, the client has disconnected.
                // Remove the client from the list and close the connection.
                lock (_lock)
                {
                    _clients.Remove(tcpClient);
                }
                tcpClient.Close();
            }
        }

        static void BroadcastMessage(string message, TcpClient excludeClient)
        {
            // Convert the message string back into bytes.
            byte[] buffer = Encoding.ASCII.GetBytes(message);

            // Lock the client list for thread-safe iteration.
            lock (_lock)
            {
                foreach (TcpClient client in _clients)
                {
                    // Send the message to every client EXCEPT the one who sent it.
                    if (client != excludeClient)
                    {
                        try
                        {
                            NetworkStream stream = client.GetStream();
                            stream.Write(buffer, 0, buffer.Length);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to send message to a client: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}