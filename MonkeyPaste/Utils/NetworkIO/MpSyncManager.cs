using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MonkeyPaste {
    #region SyncException
    public class MpSyncException : Exception {
        public MpSyncException(MpSyncMesageType errorType, Socket client, string msg) : base(msg) { }
    }
    #endregion

    public class MpSyncManager : IDisposable {
        #region Singleton
        private static readonly Lazy<MpSyncManager> _Lazy = new Lazy<MpSyncManager>(() => new MpSyncManager());
        public static MpSyncManager Instance { get { return _Lazy.Value; } }
        #endregion

        #region Private Variables
        private MpSyncMesageType _lastMessageType = MpSyncMesageType.None;
        private MpISync _localSyncData;
        private bool _isListener;
        private string _lastMsg = string.Empty;

        private string _lastSend, _lastReceive;

        //private TcpListener _listener;
        //private TcpClient _client;
        #endregion

        public event EventHandler<string> OnError;
        #region Properties
        //public MpSocketClient SocketClient { get; set; } = new MpSocketClient();
        //public MpSocketListener SocketListener { get; set; } = new MpSocketListener();

        //public MpSslTcpClient SslClient { get; set; } = new MpSslTcpClient(null);
        //public MpSslTcpServer SslServer { get; set; } = new MpSslTcpServer(null);

        //public static string EofToken { get; set; } = "<EOF>";

        public MpDeviceEndpoint ThisEndpoint { get; set; }
        public MpDeviceEndpoint OtherEndpoint { get; set; }

        //public MpSocketClient SocketClient { get; set; }
        //public MpSocketListener SocketListener { get; set; }

        public MpSessionManager SessionManager { get; set; }

        //public string StatusLog { get; set; }

        //public MpSyncMesageType LastSyncState { get; set; } = MpSyncMesageType.None;

        //public bool IsClient => SocketClient != null;
        #endregion

        #region Public Methods
        public void Init(MpISync localSync) {
            Task.Run(async () => {
                _localSyncData = localSync;
                while (!MpHelpers.Instance.IsConnectedToNetwork()) {
                    Thread.Sleep(10000);
                }
                ThisEndpoint = MpDeviceEndpointFactory.CreateEndpoint(localSync);

                //var sw = new Stopwatch();
                //sw.Start();
                //MpConsole.WriteLine(@"Attempting Listener discovery...");

                //var privateClient = await MpPrivateEndpointDiscoveryHelper.Discover(
                //    ThisEndpoint.PrivateIp4Address,
                //    ThisEndpoint.PrivatePortNum, 
                //    localSync.IsWpf());
                //sw.Stop();
                //MpConsole.WriteLine(@"Private ip sweep took {0} ms" + sw.ElapsedMilliseconds);

                if (localSync.IsWpf()) {
                    var server = new Socket(ThisEndpoint.PrivateIPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    server.Bind(ThisEndpoint.PrivateIPEndPoint);
                    server.Listen(10);
                    while(true) {
                        Socket client = null;
                        try {
                            MpConsole.WriteLine("Waiting for connection...");
                            client = server.Accept();
                            MpConsole.WriteLine("Connection made");
                            var handshakeRequest = Receive(client);
                            OtherEndpoint = MpDeviceEndpoint.Parse(handshakeRequest.Content);
                            var handshakeResponse = MpStreamMessage.CreateHandshakeResponse(ThisEndpoint, OtherEndpoint.DeviceGuid);
                            var dbLogRequest = SendReceive(client, handshakeResponse);
                            var lastSyncForOther = DateTime.Parse(dbLogRequest.Content);
                            var dbLogQueryResultStr = await _localSyncData.GetLocalLogFromSyncDate(lastSyncForOther);
                            var dbLogResponse = MpStreamMessage.CreateDbLogResponse(ThisEndpoint, OtherEndpoint.DeviceGuid, dbLogQueryResultStr);
                            var dbObjectRequest = SendReceive(client, dbLogResponse);
                            var dbObjResponseStr = await _localSyncData.GetDbObjResponseFromRequestStr(dbObjectRequest.Content);
                        }
                        catch(Exception ex) {
                            if(ex is MpSyncException) {
                                client.Shutdown(SocketShutdown.Both);
                                client.Dispose();
                                client = null;
                                MpConsole.WriteLine("Connection lost with client");
                            }
                        }
                    }
                } else {
                    var listener = ConnectSocket(@"192.168.43.209", 44381);
                    var handshakeRequest = MpStreamMessage.CreateHandshakeRequest(ThisEndpoint);
                    var handshakeResponse = SendReceive(listener,handshakeRequest);
                    OtherEndpoint = MpDeviceEndpoint.Parse(handshakeResponse.Content);
                    var lastSyncDt = await _localSyncData.GetLastSyncForRemoteDevice(OtherEndpoint.DeviceGuid);
                    var dbLogRequest = MpStreamMessage.CreateDbLogRequest(
                        ThisEndpoint,
                        OtherEndpoint.DeviceGuid,
                        lastSyncDt);
                    var dbLogResponse = SendReceive(listener, dbLogRequest);                    
                    var dbObjectRequestStr = await _localSyncData.GetDbObjRequestFromRemoteLogStr(dbLogResponse.Content);
                    var dbObjectRequest = MpStreamMessage.CreateDbObjectRequest(ThisEndpoint, OtherEndpoint.DeviceGuid, dbObjectRequestStr as string);
                    var dbObjectResponse = SendReceive(listener, dbObjectRequest);
                }
            });
        }

        private Socket ConnectSocket(string server, int port) {
            // Get host related information.
            var hostEntry = Dns.GetHostEntry(server);

            // Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
            // an exception that occurs when the host IP Address is not compatible with the address family
            // (typical in the IPv6 case).
            foreach (IPAddress address in hostEntry.AddressList) {
                IPEndPoint ipe = new IPEndPoint(address, port);
                Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                tempSocket.Connect(ipe);

                if (tempSocket.Connected) {
                   return tempSocket;
                }
            }
            return null;
        }

        private MpStreamMessage SendReceive(Socket s, MpStreamMessage smsg) {
            if (s == null) {
                return MpStreamMessage.CreateErrorMessage(ThisEndpoint, string.Empty, MpSyncMesageType.ErrorNotConnected, @"Socket is null");
            }
            if (!s.Connected) {
                return MpStreamMessage.CreateErrorMessage(ThisEndpoint, string.Empty, MpSyncMesageType.ErrorNotConnected, $"Cannot connect to socket: {s.ToString()}");
            }
            string smsgStr = smsg.SerializeDbObject();
            Byte[] bytesSent = Encoding.ASCII.GetBytes(smsgStr);
            
            MpConsole.WriteLine(@"Sending: " + smsgStr);
            s.Send(bytesSent, bytesSent.Length, SocketFlags.None);
            MpConsole.WriteLine(@"Sent: {0} bytes", bytesSent.Length);

            var responeStreamMsg = Receive(s);
            
            return responeStreamMsg;
        }

        private MpStreamMessage Receive(Socket s) {
            var bytesReceived = ReceiveAll(s);
            var response = Encoding.ASCII.GetString(bytesReceived);
            if (!response.Contains(MpStreamMessage.EofToken)) {
                if(string.IsNullOrEmpty(response)) {
                    return MpStreamMessage.CreateErrorMessage(ThisEndpoint, string.Empty, MpSyncMesageType.ErrorNotConnected, "Lost connection");
                }
                return MpStreamMessage.CreateErrorMessage(ThisEndpoint, string.Empty, MpSyncMesageType.ErrorInvalidData, $"No eof token received for msg: {response}");
            } else {
                response = response.Replace(MpStreamMessage.EofToken, string.Empty);
            }
            MpConsole.WriteLine(@"Received: " + response);
            return MpStreamMessage.Parse(response);
        }

        private byte[] ReceiveAll(Socket socket) {
            var buffer = new List<byte>();

            while(socket.Available == 0) {
                //if (!SocketConnected(socket)) {
                //    throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, socket, @"Client not connected");
                //}
            }

            while (socket.Available > 0) {
                var currByte = new Byte[1];
                var byteCounter = socket.Receive(currByte, currByte.Length, SocketFlags.None);

                if (byteCounter.Equals(1)) {
                    buffer.Add(currByte[0]);
                }
            }

            return buffer.ToArray();
        }

        private bool SocketConnected(Socket s) {
            // Exit if socket is null
            if (s == null)
                return false;
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else {
                try {
                    int sentBytesCount = s.Send(new byte[1], 1, 0);
                    return sentBytesCount == 1;
                }
                catch {
                    return false;
                }
            }
        }
        private async void Listener_OnDisconnect(object sender, DateTime e) {
            await Task.Delay(3000);
            (sender as MpSocketListenerAsync).FinishSync();
        }

        private void Client_OnConnect(object sender, string e) {
            var client = (MpSocketClientAsync)sender;
            var listenerEndpoint = MpDeviceEndpoint.Parse(e);
            string msg = string.Empty;

            Task.Run(async () => {
                //Send last sync datetime
                var lastSyncDateTime = await _localSyncData.GetLastSyncForRemoteDevice(listenerEndpoint.DeviceGuid);
                client.Send(lastSyncDateTime.ToString());

                //receive db log from listener
                msg = client.Receive();

                //send db obj request to listener
                var neededItemList = await _localSyncData.GetDbObjRequestFromRemoteLogStr(msg);
                client.Send(neededItemList.ToString());

                //receive db objects from listener
                msg = client.Receive();
                MpConsole.WriteLine(@"Objects from listener: " + msg);
                return;

                var sdt = client.FinishSync();
                MpConsole.WriteLine(@"Sync completed: " + sdt.ToString());
            });
        }

        private void Listener_OnConnect(object sender, string e) {
            var listener = (MpSocketListenerAsync)sender;
            var clientEndpoint = MpDeviceEndpoint.Parse(e);
            string msg = string.Empty;

            Task.Run(async () => {
                // receive client last sync for with listener's deviceGUid
                msg = listener.Receive();
                var clientLastSyncDateTime = DateTime.Parse(msg);
                var localSyncDateTime = await _localSyncData.GetLastSyncForRemoteDevice(OtherEndpoint.DeviceGuid);
                DateTime minLastSyncDateTime = DateTime.MinValue;
                if (localSyncDateTime > clientLastSyncDateTime) {
                    minLastSyncDateTime = clientLastSyncDateTime;
                } else {
                    minLastSyncDateTime = localSyncDateTime;
                }

                //send dblog for device guid after last sync datetime
                var localLogFromLastSync = await _localSyncData.GetLocalLogFromSyncDate(minLastSyncDateTime);
                listener.Send(localLogFromLastSync);

                //receive dbo request guid/table name pairs that client needs
                msg = listener.Receive();
                MpConsole.WriteLine(@"Client db request: " + msg);
                return;
            });
            return;
        }

        public void Dispose() {
            SessionManager.Dispose();
        }
        #endregion

        #region Private Methods
        protected string OpenMessage(string e) {
            if (!ValidateMessagee(e)) {
                throw new Exception("Message either has a bad check sum or no <Eof>");
            }
            return e.Replace(MpSocket.EofToken, string.Empty);
        }

        protected bool ValidateMessagee(string msg) {
            if (msg == null) {
                return false;
            }
            return msg.Contains(MpSocket.EofToken);
        }
        #endregion
    }
}
