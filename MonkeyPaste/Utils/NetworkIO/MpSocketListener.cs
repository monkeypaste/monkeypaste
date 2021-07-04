using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace MonkeyPaste {
    public class MpSocketListener : MpSocket, IDisposable {
        #region Private Variables
        private MpDeviceEndpoint _sep;        
        // Thread signal.  
        private ManualResetEvent _allDone = new ManualResetEvent(false);
        #endregion

        #region Properties
        public List<string> OutMessageQueue = new List<string>();
        #endregion

        #region Events
        public event EventHandler<string> OnSend;
        public event EventHandler<string> OnReceive;
        public event EventHandler<string> OnError;
        #endregion

        #region Public Methods
        public MpSocketListener(MpDeviceEndpoint sep) {
            _sep = sep;
        }

        public async Task Start2(MpISync syncData) {
            if(IsRunning) {
                return;
            }
            TcpListener server = null;
            try {
                server = new TcpListener(IPAddress.Any, _sep.PublicPortNum);

                MpConsole.WriteLine($"Starting Listener with endpoint: {_sep}");
                server.Start();

                IsRunning = true;

                TcpClient client = null;
                // Enter the listening loop.
                while (IsRunning) {
                    Console.Write("Waiting for a connection... ");
                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    if(client == null) {
                        client = server.AcceptTcpClient();
                        Console.WriteLine("Connected!");
                    }

                    int i;
                    var readBytes = new Byte[256];
                    var sb = new StringBuilder();
                    var stream = client.GetStream();
                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(readBytes, 0, readBytes.Length)) != 0) {
                        // Translate data bytes to a ASCII string.
                        string inStr = System.Text.Encoding.ASCII.GetString(readBytes, 0, i);
                        sb.Append(inStr);
                    }
                    string response = sb.ToString();
                    Console.WriteLine("Listener Received: {0}", response);
                    OnReceive?.Invoke(this, response);

                    foreach(var outMsg in OutMessageQueue) {
                        var outBytes = System.Text.Encoding.ASCII.GetBytes(outMsg);
                        stream.Write(outBytes, 0, outBytes.Length);
                    }
                    OutMessageQueue.Clear();
                }
                // Shutdown and end connection
                client.Close();
            }
            catch (SocketException e) {
                Console.WriteLine("SocketException: {0}", e);
            } finally {
                // Stop listening for new clients.
                server.Stop();
            }
        }
        public void Stop() {
            IsRunning = false;
        }
        public void StartListening() {
            // Create a TCP/IP socket.  
            var listener = new Socket(
                //IPAddress.Any.AddressFamily, 
                _sep.IPEndPoint.AddressFamily,
                SocketType.Stream, 
                ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try {
                listener.Bind(_sep.IPEndPoint);
                listener.Listen(100);
                IsRunning = true;
                while (IsRunning) {
                    // Set the event to nonsignaled state.  
                    _allDone.Reset();
                    // Start an asynchronous socket to listen for connections.  
                    MpConsole.WriteLine($"Waiting for a connection ({_sep}) ...");                    
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    // Wait until a connection is made before continuing.  
                    _allDone.WaitOne();
                }

            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        public void Dispose() {
            //throw new NotImplementedException();
        }
        #endregion

        #region Private Methods
        private void AcceptCallback(IAsyncResult ar) {
            // Get the socket that handles the client request.  
            var listener = (Socket)ar.AsyncState;
            var handler = listener.EndAccept(ar);

            // Signal the main thread to continue.  
            _allDone.Set();

            // Create the state object.  
            var state = new MpSocketStateObject();
            state.WorkSocket = handler;
            handler.BeginReceive(state.Buffer,0,
                MpSocketStateObject.BufferSize,
                0,
                new AsyncCallback(ReadCallback),
                state);
        }

        private void ReadCallback(IAsyncResult ar) {
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            MpSocketStateObject state = (MpSocketStateObject)ar.AsyncState;
            Socket listener = state.WorkSocket;

            if(!listener.Connected) {
                return;
            }
            string content = string.Empty;
            // Read data from the client socket.
            int bytesRead = listener.EndReceive(ar);
            if (bytesRead > 0) {
                // There  might be more data, so store the data received so far.  
                state.Sb.Append(Encoding.ASCII.GetString(
                    state.Buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read
                // more data.  
                content = state.Sb.ToString();
                if (content.IndexOf("<EOF>") > -1) {
                    // All the data has been read from the
                    // client. Display it on the console.  
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",content.Length, content);

                    // Echo content checksum back to client 
                    Send(listener, content.CheckSum());

                } else {
                    // Not all data received. Get more.  
                    listener.BeginReceive(state.Buffer, 0, MpSocketStateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }            
        }

        private void Send(Socket handler, String data) {
            data += EofToken;
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            Console.WriteLine(@"Sending '{0}' to client.. ", data);
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

                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
                OnSend?.Invoke(this, string.Empty);

            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                OnError?.Invoke(this, e.ToString());
            }
        }
        #endregion
    }
}
