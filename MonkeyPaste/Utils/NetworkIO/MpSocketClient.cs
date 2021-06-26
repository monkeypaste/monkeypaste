using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Diagnostics;

namespace MonkeyPaste {
    public class MpSocketClient : IDisposable {
        #region Private Variables
        //private MpSocketStateObject _sso = null;

        // ManualResetEvent instances signal completion.  
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);

        // The response from the remote device.  
        private string response = String.Empty;

        //private Socket _client = null;

        private string _hostIp;
        private int _hostPort;
        #endregion

        #region Events
        public event EventHandler<string> OnReceive;
        #endregion

        #region Public Methods

        public void StartClient(string hostIp, int hostPort) {
            _hostIp = hostIp;
            _hostPort = hostPort;
            try {
                //_client = Connect();

                // Send test data to the remote device.  
                Send("This is a test<EOF>");
                sendDone.WaitOne();

                // Receive the response from the remote device.  
                Receive();
                receiveDone.WaitOne();

                // Write the response to the console.  
                Console.WriteLine("Response received : {0}", response);

                Disconnect();
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

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
            // The name of the
            // remote device is "host.contoso.com".  
            var ipHostInfo = Dns.GetHostEntry(_hostIp);
            IPAddress ipAddress = null;
            foreach (var ipa in ipHostInfo.AddressList) {
                if (ipa.ToString() == _hostIp) {
                    ipAddress = ipa;
                }
            }
            if (ipAddress == null) {
                throw new Exception($"Unable to connect to socket server at {_hostIp}:{_hostPort}");
            }
            var remoteEP = new IPEndPoint(ipAddress, _hostPort);
            // Create a TCP/IP socket.  
            var client = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            // Connect to the remote endpoint.  
            client.BeginConnect(
                remoteEP,
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
            }
        }

        private void Receive() {
            var client = Connect();
            Debug.Assert(client != null, @"Cannot receive client is null");
            try {
                // Create the state object.  
                MpSocketStateObject state = new MpSocketStateObject(_hostPort);
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
                    if (state.sb.Length > 1) {
                        response = state.sb.ToString();
                    }
                    // Signal that all bytes have been received.  
                    receiveDone.Set();

                    OnReceive?.Invoke(this, response);
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
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
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
        #endregion
    }
}