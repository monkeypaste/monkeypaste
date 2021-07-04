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
    public class MpSocketClient : MpSocket, IDisposable {
        #region Private Variables
        // ManualResetEvent instances signal completion.  
        private ManualResetEvent connectDone = new ManualResetEvent(false);

        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent confirmSendDone = new ManualResetEvent(false);

        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        private ManualResetEvent confirmReceiveDone = new ManualResetEvent(false);

        private MpDeviceEndpoint _sep, _cep;        
        #endregion

        #region Events
        public event EventHandler<string> OnSend;
        public event EventHandler<string> OnReceive;
        public event EventHandler<string> OnError;
        #endregion

        #region Public Methods
        public static async Task<MpSocketClient> TryPrivateCreateAndConnect(MpDeviceEndpoint thisEndpoint, bool isWpf) {
            var sc = new MpSocketClient(thisEndpoint);
            var sep = await sc.FindPrivateListener(isWpf);
            if(sep != null) {
                thisEndpoint.IsPublic = false;
                sep.IsPublic = false;
                return new MpSocketClient(thisEndpoint, sep);
            }
            return null;
        }

        private MpSocketClient(MpDeviceEndpoint thisEndPoint) {
            _cep = thisEndPoint;
        }
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

        
        public void StartClient() {
            // Connect to a remote device.  
            try {
                // Create a TCP/IP socket.  
                Socket client = new Socket(_cep.IPEndPoint.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                client.BeginConnect(_sep.IPEndPoint,
                    new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();
                IsRunning = true;
                MpConsole.WriteLine($"Client ({_cep}) Connected to Listener ({_sep})");
                // Send test data to the remote device.  
                Send(client, "This is a test");
                sendDone.WaitOne();

                // Receive the response from the remote device.  
                Receive(client);
                receiveDone.WaitOne();

                // Write the response to the console.  
                Console.WriteLine("Response received : {0}", _lastReceive);

                while (IsRunning) { 
                    
                }
                // Release the socket.  
                //client.Shutdown(SocketShutdown.Both);
               // client.Close();

            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
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

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        private void Receive(Socket client) {
            try {
                // Create the state object.  
                MpSocketStateObject state = new MpSocketStateObject();
                state.WorkSocket = client;

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.Buffer, 0, MpSocketStateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar) {
            try {
                // Retrieve the state object and the client socket
                // from the asynchronous state object.  
                MpSocketStateObject state = (MpSocketStateObject)ar.AsyncState;
                Socket client = state.WorkSocket;

                string content = string.Empty;
                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0) {
                    // There might be more data, so store the data received so far.  
                    state.Sb.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));

                    content = state.Sb.ToString();
                    if (content.IndexOf("<EOF>") > -1) {
                        // All the data has been read from the
                        // client. Display it on the console.  
                        Console.WriteLine("Read {0} bytes from socket. \n Data : {1}", content.Length, content);

                        _lastReceive = content;
                        // Signal that all bytes have been received.  
                        receiveDone.Set();

                        // Echo content checksum back to client 
                        //Send(client, content.CheckSum());

                    } else {
                        // Not all data received. Get more.  
                        client.BeginReceive(state.Buffer, 0, MpSocketStateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                    }
                } 
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        private void Send(Socket client, String data) {
            data += EofToken;

            _lastSend = data;
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
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        private string TestConnect(string ip, int port, bool isWpf) {
            if(!isWpf) {
                if (test_connection_mobile_tcp(ip, port)) {
                    return ip;
                }
                return null;
            } 
            if (test_connection_wpf(ip, port)) {
                return ip;
            }
            return null;
        }

        public bool test_connection_wpf(String hostname, int portno) {
            bool connected = true;
            using(var tcp = new TcpClient()) {
                IAsyncResult ar = tcp.BeginConnect(hostname, portno, null, null);
                System.Threading.WaitHandle wh = ar.AsyncWaitHandle;
                try {
                    if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(100), false)) {
                        //Console.WriteLine("Tried " + hostname);
                        tcp.Close();
                        connected = false;
                    } else {
                        tcp.EndConnect(ar);
                    }
                } finally {
                    wh.Close();
                }
            }            
            return connected;
        }
        private bool test_connection_mobile_tcp(string hostname, int portno) {
            using (var tcpc = new TcpClient()) {
                tcpc.SendTimeout = 25;
                tcpc.ReceiveTimeout = 25;
                try {
                    tcpc.Connect(hostname, portno);
                    if (tcpc.Connected) {
                        tcpc.Close();
                        return true;
                    }
                    tcpc.Close();

                }
                catch (System.Net.Sockets.SocketException) {
                    if (tcpc != null) {
                        tcpc.Close();
                    }
                    return false;
                }
            }
            return false;
        }
        
        private bool test_connection_mobile(string hostname, int portno) {
            
            IPAddress ipa = (IPAddress)Dns.GetHostAddresses(hostname)[0];
           using (Socket sock = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp)) {
                    try {
                    sock.SendTimeout = 25;
                    sock.Connect(ipa, portno); 
                    if (sock.Connected) {
                        sock.Close();
                        return true;
                    }
                    sock.Close();

                }
                catch (System.Net.Sockets.SocketException) {
                    if (sock != null) {
                        sock.Close();
                    }
                    return false;
                }
            }            
            return false;
        }

        private async Task<MpDeviceEndpoint> FindPrivateListener(bool isWpf) {
            // TODO need to test in different network scenerios may need use
            // private helper string[] GetAllIp4 and scan all local up addresses
            // on LAN's with actual routers...
            var thisIp = _cep.PrivateIp4Address;
            var ipParts = thisIp.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            string ipPrefix = string.Format("{0}.{1}.{2}.", ipParts[0], ipParts[1], ipParts[2]);
            var taskList = new List<Task<string>>();
            for (int i = 1; i < 255; i++) {
                string b4 = i.ToString();
                if (b4 == ipParts[3]) {
                    //skip this ip
                    continue;
                }
                string ip = ipPrefix + b4;
                if (TestConnect(ip, _cep.PrivatePortNum,isWpf) != null) {
                    return new MpDeviceEndpoint() {
                        PrivateIp4Address = ip,
                        PrivatePortNum = _cep.PrivatePortNum,
                        IsPublic = false
                    };
                } else {
                    await Task.Delay(0);
                }
            }
            return null;
        }
#endregion
    }
}