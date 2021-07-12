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
    public class MpSocketListenerAsync : MpSocket, IDisposable {
        #region Private Variables
        private int _curClientIdx = 0;
        private Socket _client;

        private ObservableCollection<Socket> _clients = new ObservableCollection<Socket>();

        private MpDeviceEndpoint _sep,_cep;        

        private ManualResetEvent _allDone = new ManualResetEvent(false);
        private ManualResetEvent _connectDone = new ManualResetEvent(false);
        private ManualResetEvent _sendDone = new ManualResetEvent(false);
        private ManualResetEvent _receiveDone = new ManualResetEvent(false);

        private ManualResetEvent _syncDone = new ManualResetEvent(false);
        #endregion

        #region Properties
        

        #endregion

        #region Events
        public event EventHandler<string> OnConnect;
        public event EventHandler<string> OnReceive;
        public event EventHandler<string> OnSend;
        public event EventHandler<string> OnError; 
        public event EventHandler<DateTime> OnDisconnect;
        #endregion

        #region Public Methods
        public MpSocketListenerAsync(MpDeviceEndpoint sep, MpISync localSync) {
            _sep = sep;
            _localSync = localSync;
        }

        public async Task Start2(MpISync syncData) {
            if(IsRunning) {
                return;
            }
            TcpListener server = null;
            try {
                server = new TcpListener(IPAddress.Any, _sep.PublicConnectPortNum);

                MpConsole.WriteLine($"Starting Listener with endpoint: {_sep}");
                server.Start();

                IsRunning = true;

                TcpClient client = null;
                // Enter the listening loop.
                while (IsRunning) {
                    MpConsole.WriteLine("Waiting for a connection... ");
                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    if(client == null) {
                        client = server.AcceptTcpClient();
                        MpConsole.WriteLine("Connected!");
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

                    //foreach(var outMsg in OutMessageQueue) {
                    //    var outBytes = System.Text.Encoding.ASCII.GetBytes(outMsg);
                    //    stream.Write(outBytes, 0, outBytes.Length);
                    //}
                    //OutMessageQueue.Clear();
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
                _sep.PrivateConnectIPEndPoint.AddressFamily,
                SocketType.Stream, 
                ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try {
                listener.Bind(_sep.PrivateConnectIPEndPoint);
                listener.Listen(100);
                IsRunning = true;
                
                while (IsRunning) {
                    // Set the event to nonsignaled state.  
                    _allDone.Reset();
                    _receiveDone.Reset();
                    _sendDone.Reset();
                    // Start an asynchronous socket to listen for connections.  
                    MpConsole.WriteLine($"Waiting for a connection ({_sep}) ...");                    
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    // Wait until a connection is made before continuing.  
                    _allDone.WaitOne();                    

                    if(_client != null) {
                        string cepStr = Receive();
                        _cep = MpDeviceEndpoint.Parse(cepStr);

                        _sep.ConnectDateTime = DateTime.UtcNow;
                        Send(_sep.SerializeDbObject());

                        string lsdt = Receive();
                        var clientLastSyncDateTime = DateTime.Parse(lsdt);
                        var localSyncDateTime = _localSync.GetLastSyncForRemoteDevice(_cep.DeviceGuid).Result;
                        DateTime minLastSyncDateTime = DateTime.MinValue;
                        if (localSyncDateTime > clientLastSyncDateTime) {
                            minLastSyncDateTime = clientLastSyncDateTime;
                        } else {
                            minLastSyncDateTime = localSyncDateTime;
                        }

                        //send dblog for device guid after last sync datetime
                        var localLogFromLastSync = _localSync.GetLocalLogFromSyncDate(minLastSyncDateTime).Result;
                        Send(localLogFromLastSync);

                        //receive dbo request guid/table name pairs that client needs
                        string dboReq = Receive();
                        MpConsole.WriteLine(@"Client db request: " + dboReq);
                        
                        //_receiveDone.WaitOne();

                        //OnConnect?.Invoke(this, OpenMessage(_lastReceive));
                        //_syncDone.WaitOne();
                    }                    
                }

            }
            catch (Exception e) {
                MpConsole.WriteLine(e.ToString());
                OnError?.Invoke(this, @"Server start exception: " + e.ToString());
            }
        }

        public void FinishSync() {
            _clients.Add(_client);
            _client = null;
            _syncDone.Set();
        }

        public void Send(string msg) {
            Send(_client, msg);
            _sendDone.WaitOne();
        }

        public string Receive() {
            _lastReceive = string.Empty;
            Receive(_client);
            _receiveDone.WaitOne();
            return OpenMessage(_lastReceive);
        }

        public void Dispose() {
            //throw new NotImplementedException();
        }
        #endregion

        #region Private Methods
        private void AcceptCallback(IAsyncResult ar) {
            try {
                // Get the socket that handles the client request.  
                var listener = (Socket)ar.AsyncState;
                var handler = listener.EndAccept(ar);
                _client = handler;
                _allDone.Set();

                // Create the state object.  
                var state = new MpSocketStateObject();
                state.WorkSocket = handler;
                handler.BeginReceive(state.Buffer, 0,
                    MpSocketStateObject.BufferSize,
                    0,
                    new AsyncCallback(ReceiveCallback),
                    state);
                //OnConnect?.Invoke(this, (++_clientCount).ToString());
            }
            catch(Exception ex) {
                OnError?.Invoke(this, @"Server accept callback exception: " + ex.ToString());
            }            
        }

        private void Receive(Socket client) {
            try {
                // Create the state object.  
                MpSocketStateObject state = new MpSocketStateObject();
                state.WorkSocket = client;
                //while (client.Available == 0) {
                //    Thread.Sleep(10);
                //}
                // Begin receiving the data from the remote device.  
                client.BeginReceive(
                    state.Buffer, 
                    0, 
                    MpSocketStateObject.BufferSize, 
                    0,
                    new AsyncCallback(ReceiveCallback), 
                    state);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar) {
            try {
                // Retrieve the state object and the handler socket  
                // from the asynchronous state object.  
                MpSocketStateObject state = (MpSocketStateObject)ar.AsyncState;
                Socket listener = state.WorkSocket;

                // Read data from the client socket.
                int bytesRead = listener.EndReceive(ar);
                _client = listener;

                if (bytesRead > 0) {
                    // There  might be more data, so store the data received so far.  
                    _lastReceive += Encoding.ASCII.GetString(state.Buffer, 0, bytesRead);

                    if (_lastReceive.IndexOf(EofToken) > -1) {
                        MpConsole.WriteLine(@"Read {0} bytes from socket. \n Data : {1}", _lastReceive.Length, _lastReceive);
                        _receiveDone.Set();

                    } else if (_lastReceive.IndexOf(EosToken) > -1) {
                        MpConsole.WriteLine(@"Read {0} bytes from socket. \n Data : {1}", _lastReceive.Length, _lastReceive);
                        
                        var syncDt = DateTime.Parse(OpenMessage(_lastReceive));
                        Send(_client, syncDt.ToString(), true);
                        _sendDone.WaitOne();

                        OnDisconnect?.Invoke(this, syncDt);
                        _receiveDone.Set();

                    } else {
                        // Not all data received. Get more.  
                        listener.BeginReceive(state.Buffer, 0, MpSocketStateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                    }
                }
            }
            catch (Exception ex) {
                OnError?.Invoke(this, @"Server Read Callback exception:" + ex.ToString());
            }
        }

        private void Send(Socket handler, String data, bool isLastMessage = false) {
            data += isLastMessage ? EosToken : EofToken;

            _lastSend = data;
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
                _client = handler;
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
                //OnSend?.Invoke(this, _lastSend);
                _sendDone.Set();

            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                OnError?.Invoke(this, @"Server send callback exception: " + e.ToString());
            }
        }
        #endregion
    }
}
