using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpSocketListener : IDisposable {
        #region Private Variables
        
        private int _port;
        // Thread signal.  
        private ManualResetEvent _allDone = new ManualResetEvent(false);
        #endregion

        #region Properties
        public bool IsRunning { get; set; } = false;
        #endregion

        #region Events

        public event EventHandler<string> OnReceive;
        #endregion

        #region Public Methods
        public MpSocketListener() { }

        public void Start(MpDeviceEndpoint thisEndpoint) {
            Task.Run(() => {
                TcpListener server = null;
                try {
                    // Set the TcpListener on port 13000.
                    Int32 port = thisEndpoint.PortNum;
                    //IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                                         
                    //server = new TcpListener(port);
                    server = new TcpListener(IPAddress.IPv6Any, port);
                    server.AllowNatTraversal(true);

                    // Start listening for client requests.
                    server.Start();

                    // Buffer for reading data
                    Byte[] bytes = new Byte[256];
                    String data = null;

                    IsRunning = true;
                    // Enter the listening loop.
                    while (IsRunning) {
                        Console.Write("Waiting for a connection... ");

                        // Perform a blocking call to accept requests.
                        // You could also use server.AcceptSocket() here.
                        TcpClient client = server.AcceptTcpClient();
                        Console.WriteLine("Connected!");

                        data = null;

                        // Get a stream object for reading and writing
                        NetworkStream stream = client.GetStream();

                        int i;
                        var sb = new StringBuilder();
                        // Loop to receive all the data sent by the client.
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0) {
                            // Translate data bytes to a ASCII string.
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                            Console.WriteLine("Received: {0}", data);

                            // Process the data sent by the client.
                            //data = data.ToUpper();
                            sb.Append(data);

                            byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                            // Send back a response.
                            stream.Write(msg, 0, msg.Length);
                            Console.WriteLine("Sent: {0}", data);
                        }
                        OnReceive?.Invoke(this, sb.ToString());
                        // Shutdown and end connection
                        client.Close();
                    }
                }
                catch (SocketException e) {
                    Console.WriteLine("SocketException: {0}", e);
                } finally {
                    // Stop listening for new clients.
                    server.Stop();
                }

                Console.WriteLine("\nHit enter to continue...");
                Console.Read();
            });            
        }

        public void Stop() {
            IsRunning = false;
        }
        public void StartListening(int port) {
            _port = port;
            //_sso = new MpSocketStateObject(ip, port);

            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // running the listener is "host.contoso.com".  
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(
                IPAddress.Any.AddressFamily, 
                SocketType.Stream, 
                ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true) {
                    // Set the event to nonsignaled state.  
                    _allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    // Wait until a connection is made before continuing.  
                    _allDone.WaitOne();
                }

            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public void Dispose() {
            //throw new NotImplementedException();
        }
        #endregion

        #region Private Methods
        private void AcceptCallback(IAsyncResult ar) {
            // Signal the main thread to continue.  
            _allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.  
            MpSocketStateObject state = new MpSocketStateObject(_port);
            //_sso.workSocket = handler;
            handler.BeginReceive(
                state.buffer,
                0,
                MpSocketStateObject.BufferSize,
                0,
                new AsyncCallback(ReadCallback),
                state);
        }

        private void ReadCallback(IAsyncResult ar) {
            String content = String.Empty;

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            MpSocketStateObject state = (MpSocketStateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket.
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0) {
                //var byteArray = await response.Content.ReadAsByteArrayAsync();
                //_accessToken = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);

                // There  might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read
                // more data.  
                content = state.sb.ToString();
                if (content.IndexOf(MpSocketStateObject.EofToken) > -1) {
                    // All the data has been read from the
                    // client. Display it on the console.  
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);

                    OnReceive?.Invoke(this, content);
                    // Echo the data back to the client.  
                    Send(handler, content);
                } else {
                    // Not all data received. Get more.  
                    handler.BeginReceive(
                        state.buffer,
                        0,
                        MpSocketStateObject.BufferSize,
                        0,
                        new AsyncCallback(ReadCallback),
                        state);
                }
            }
        }

        private void Send(Socket handler, String data) {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar) {
            try {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
        #endregion
    }
}
