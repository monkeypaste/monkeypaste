using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.UI.Views;
using System.Net.NetworkInformation;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpSocketClientAsync : MpSocket, IDisposable {
        #region Private Variables
        //private MpSocketStateObject _sso = null;
        // ManualResetEvent instances signal completion.  
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        //private ManualResetEvent syncDone = new ManualResetEvent(false);

        private Socket _listener,_client;

        private MpDeviceEndpoint _sep, _cep;
        #endregion

        #region Events
        public event EventHandler<string> OnConnect;
        public event EventHandler<string> OnReceive;
        public event EventHandler<string> OnSend;
        public event EventHandler<string> OnError;
        #endregion

        #region Properties
        public MpDeviceEndpoint ListenerEndpoint {
            get {
                return _sep;
            }
        }
        #endregion

        #region Public Methods
        public MpSocketClientAsync(MpDeviceEndpoint thisEndPoint, MpDeviceEndpoint listenerEndPoint, MpISync localSync) {
            _cep = thisEndPoint;
            _sep = listenerEndPoint;
            _localSync = localSync;
        }      
        
        public void StartClient() {
            // Connect to a remote device.  
            try {
                // Create a TCP/IP socket.  
                Socket client = new Socket(
                    _cep.PrivateIPEndPoint.AddressFamily,
                    SocketType.Stream, 
                    ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                client.BeginConnect(
                    _sep.PrivateIPEndPoint,
                    new AsyncCallback(ConnectCallback), 
                    client);
                connectDone.WaitOne();
                
                _client = client;
                IsRunning = true;
                MpConsole.WriteLine($"Client ({_cep}) Connected to Listener ({_sep})");
                
                if(_listener != null) {
                    // Send test data to the remote device.  
                    _cep.ConnectDateTime = DateTime.UtcNow;
                    Send(_cep.SerializeDbObject());

                    // Receive the response from the remote device.  
                    string sepStr = Receive();
                    _sep = MpDeviceEndpoint.Parse(sepStr);

                    //Send last sync datetime
                    var lastSyncDateTime = _localSync.GetLastSyncForRemoteDevice(_sep.DeviceGuid).Result;
                    Send(lastSyncDateTime.ToString());

                    //receive db log from listener
                    string remoteDbLog = Receive();

                    //send db obj request to listener
                    var neededItemList = _localSync.ProcessRemoteDbLog(remoteDbLog).Result;
                    Send(neededItemList.ToString());

                    //receive db objects from listener
                    string dbObjStr = Receive();
                    MpConsole.WriteLine(@"Objects from listener: " + dbObjStr);
                }

            }
            catch (Exception e) {
                MpConsole.WriteLine(e.ToString());
            }
        }
        public void Send(string msg) {
            Send(_listener, msg);
            sendDone.WaitOne();
        }

        public string Receive() {
            _lastReceive = string.Empty;
            Receive(_listener);
            receiveDone.WaitOne();
            return OpenMessage(_lastReceive);
        }

        public DateTime FinishSync() {
            var finishSyncDateTime = DateTime.UtcNow;
            Send(_listener, finishSyncDateTime.ToString(), true);
            sendDone.WaitOne();

            Receive(_listener);
            receiveDone.WaitOne();

            var confirmFinishSyncDateTime = DateTime.Parse(OpenMessage(_lastReceive));

            if(!finishSyncDateTime.Equals(confirmFinishSyncDateTime)) {
                throw new Exception("Sync history dt must match");
            }
            Disconnect();

            return finishSyncDateTime;
        }

        public void Disconnect() {
            // Release the socket.  
            _client.Shutdown(SocketShutdown.Both);
            _client.Close();
        }

        public void Dispose() {
        }
        #endregion
        
        #region Private Methods
        private void ConnectCallback(IAsyncResult ar) {
            try {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                _listener = client;
                MpConsole.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e) {
                MpConsole.WriteLine(e.ToString());
                OnError?.Invoke(this, @"Client Connect Callback exception:" + e.ToString());
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
                client.BeginReceive(state.Buffer, 0, MpSocketStateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e) {
                MpConsole.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar) {
            try {
                // Retrieve the state object and the client socket
                // from the asynchronous state object.  
                MpSocketStateObject state = (MpSocketStateObject)ar.AsyncState;
                Socket client = state.WorkSocket;


                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);
                _listener = client;
                if (bytesRead > 0) {
                    // There might be more data, so store the data received so far.  
                    _lastReceive += Encoding.ASCII.GetString(state.Buffer, 0, bytesRead);
                    if (_lastReceive.IndexOf(EofToken) > -1) {
                        MpConsole.WriteLine(@"Read {0} bytes from socket. \n Data : {1}", _lastReceive.Length, _lastReceive);
                        receiveDone.Set();
                    } else if (_lastReceive.IndexOf(EosToken) > -1) {
                        MpConsole.WriteLine(@"Listener has confirmed sync is completed with dt: "+ _lastReceive);
                        receiveDone.Set();

                    } else {
                        // Not all data received. Get more.  
                        client.BeginReceive(state.Buffer, 0, MpSocketStateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                    }
                } 
            }
            catch (Exception e) {
                MpConsole.WriteLine(e.ToString());
                OnError?.Invoke(this, @"Client ReceiveCallback exception:" + e.ToString());
            }
        }
        private void Send(Socket client, String data, bool isLastMessage = false) {
            data += isLastMessage ? EosToken : EofToken;
            _lastSend = data;
            MpConsole.WriteLine(@"Sending {0} to listener", data);
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar) {
            try {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                MpConsole.WriteLine("Sent {0} bytes to server. Data: {1}");

                // Signal that all bytes have been sent.  
                sendDone.Set();

                OnSend?.Invoke(this, _lastSend);
            }
            catch (Exception e) {
                MpConsole.WriteLine(e.ToString());
                OnError?.Invoke(this, @"Client SendCallback exception:" + e.ToString());
            }
        }
#endregion
    }
}