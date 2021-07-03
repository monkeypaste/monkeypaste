using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace MonkeyPaste {
    public class MpSocketClient : IDisposable {
        #region Private Variables
        // ManualResetEvent instances signal completion.  
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);

        private MpDeviceEndpoint _sep, _cep;

        
        #endregion

        #region Events
        public event EventHandler<string> OnSend;
        public event EventHandler<string> OnReceive;
        public event EventHandler<string> OnError;
        #endregion

        #region Public Methods
        public MpSocketClient(MpDeviceEndpoint thisEndPoint, MpDeviceEndpoint listenerEndPoint) {
            _cep = thisEndPoint;
            _sep = listenerEndPoint;
        }

        public void SendMessage(string message) {
            try {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer
                // connected to the same address as specified by the server, port
                // combination.
                MpConsole.WriteLine($"Connecting to remote endpoint {_sep.ToString()}");
                TcpClient client = new TcpClient(_sep.Ip4Address, _sep.PortNum);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] outBytes = System.Text.Encoding.ASCII.GetBytes(message);

                Byte[] inBytes = new Byte[256];
                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer.
                stream.Write(outBytes, 0, outBytes.Length);

                Console.WriteLine("Sent: {0}", message);
                while (true) {
                    int i;
                    var sb = new StringBuilder();
                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(inBytes, 0, inBytes.Length)) != 0) {
                        // Translate data bytes to a ASCII string.
                        string inStr = System.Text.Encoding.ASCII.GetString(inBytes, 0, i);
                        sb.Append(inStr);
                    }
                    string response = sb.ToString();
                    Console.WriteLine("Client Received: {0}", response);
                    OnReceive?.Invoke(this, response);
                }
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e) {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e) {
                Console.WriteLine("SocketException: {0}", e);
            }

            //Console.WriteLine("\n Press Enter to continue...");
            //Console.Read();
        }

        //public void Send(MpDeviceEndpoint sep, String message) {
        //    try {
        //        // Create a TcpClient.
        //        // Note, for this client to work you need to have a TcpServer
        //        // connected to the same address as specified by the server, port
        //        // combination.
        //        TcpClient client = new TcpClient(sep.IPEndPoint);

        //        // Translate the passed message into ASCII and store it as a Byte array.
        //        Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

        //        // Get a client stream for reading and writing.
        //        //  Stream stream = client.GetStream();

        //        NetworkStream stream = client.GetStream();

        //        // Send the message to the connected TcpServer.
        //        stream.Write(data, 0, data.Length);

        //        Console.WriteLine("Sent: {0}", message);

        //        // Receive the TcpServer.response.

        //        // Buffer to store the response bytes.
        //        data = new Byte[256];

        //        // String to store the response ASCII representation.
        //        String responseData = String.Empty;

        //        // Read the first batch of the TcpServer response bytes.
        //        Int32 bytes = stream.Read(data, 0, data.Length);
        //        responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
        //        Console.WriteLine("Received: {0}", responseData);

        //        // Close everything.
        //        stream.Close();
        //        client.Close();
        //    }
        //    catch (ArgumentNullException e) {
        //        Console.WriteLine("ArgumentNullException: {0}", e);
        //    }
        //    catch (SocketException e) {
        //        Console.WriteLine("SocketException: {0}", e);
        //    }

        //    Console.WriteLine("\n Press Enter to continue...");
        //    Console.Read();
        //}

        //public void StartClient(string hostIp, int hostPort) {
        //    _hostIp = hostIp;
        //    _hostPort = hostPort;
        //    try {
        //        //_client = Connect();

        //        // Send test data to the remote device.  
        //        Send("This is a test<EOF>");
        //        sendDone.WaitOne();

        //        // Receive the response from the remote device.  
        //        Receive();
        //        receiveDone.WaitOne();

        //        // Write the response to the console.  
        //        Console.WriteLine("Response received : {0}", response);

        //        Disconnect();
        //    }
        //    catch (Exception e) {
        //        Console.WriteLine(e.ToString());
        //    }
        //}

        public void Disconnect() {
            var client = Connect();
            if(client != null) {
                // Release the socket.  
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
        }

        public void Send(string data) {
            var client = Connect();

            Debug.Assert(client != null, "Cannot send client is null");
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        public void Dispose() {
            Disconnect();
        }
        #endregion

        #region Private Methods

        private Socket Connect() {
            // Establish the remote endpoint for the socket.  
            // Create a TCP/IP socket.  
            var client = new Socket(
                _sep.IPEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            // Connect to the remote endpoint.  
            client.BeginConnect(
                _sep.IPEndPoint,
                new AsyncCallback(ConnectCallback),
                client);

            connectDone.WaitOne();

            return client;
        }

        private void ConnectCallback(IAsyncResult ar) {
            try {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine(
                    "Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                OnError?.Invoke(this, e.ToString());
            }
        }
        private void SendCallback(IAsyncResult ar) {
            try {
                // Retrieve the socket from the state object.  
                var client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                sendDone.Set();
                OnSend?.Invoke(this, string.Empty);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                OnError?.Invoke(this, e.ToString());
            }
        }

        private void Receive() {
            var client = Connect();
            Debug.Assert(client != null, @"Cannot receive client is null");
            try {
                // Create the state object.  
                MpSocketStateObject state = new MpSocketStateObject(_sep.PublicPortNum);
                state.workSocket = client;

                // Begin receiving the data from the remote device.  
                client.BeginReceive(
                    state.buffer, 
                    0, 
                    MpSocketStateObject.BufferSize, 
                    0,
                    new AsyncCallback(ReceiveCallback), 
                    state);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                OnError?.Invoke(this, e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar) {
            try {
                // Retrieve the state object and the client socket
                // from the asynchronous state object.  
                var state = (MpSocketStateObject)ar.AsyncState;
                var client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0) {
                    // There might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.  
                    client.BeginReceive(
                        state.buffer, 
                        0, 
                        MpSocketStateObject.BufferSize, 
                        0,
                        new AsyncCallback(ReceiveCallback), 
                        state);
                } else {
                    // All the data has arrived; put it in response.  
                    // Signal that all bytes have been received.  
                    receiveDone.Set();

                    OnReceive?.Invoke(this, state.sb.ToString());
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                OnError?.Invoke(this, e.ToString());
            }
        }

        #endregion
    }
}