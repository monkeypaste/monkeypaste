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
            localSync.OnSyncableChange += LocalSync_OnSyncableChange;

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
                //return;
                if (localSync.IsWpf()) {
                    var server = new Socket(ThisEndpoint.PrivateConnectIPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    server.Bind(ThisEndpoint.PrivateConnectIPEndPoint);
                    server.Listen(10); 
                    while(true) {
                        Socket client = null;
                        try {
                            MpConsole.WriteLine("Waiting for connection...");
                            client = server.Accept();
                            MpConsole.WriteLine("Connection made");

                            //var dbFileResponse = Receive(client);
                            //MpHelpers.Instance.WriteByteArrayToFile(
                            //    @"C:\\Users\tkefauver\Desktop\android.db",
                            //    Convert.FromBase64String(dbFileResponse.Content));

                            //return;
                            var handshakeRequest = Receive(client);
                            OtherEndpoint = MpDeviceEndpoint.Parse(handshakeRequest.Content);
                            var handshakeResponse = MpStreamMessage.CreateHandshakeResponse(ThisEndpoint, OtherEndpoint.DeviceGuid);
                            var dbLogRequest = SendReceive(client, handshakeResponse);
                            var lastSyncForOther = DateTime.Parse(dbLogRequest.Content);
                            var dbLogQueryResultStr = await _localSyncData.GetLocalLogFromSyncDate(lastSyncForOther);
                            var dbLogResponse = MpStreamMessage.CreateDbLogResponse(ThisEndpoint, OtherEndpoint.DeviceGuid, dbLogQueryResultStr);
                            
                            var flipRequest = SendReceive(client, dbLogResponse);
                            var thisLastSyncDt = await _localSyncData.GetLastSyncForRemoteDevice(OtherEndpoint.DeviceGuid);
                            var thisDbLogRequest = MpStreamMessage.CreateDbLogRequest(
                                ThisEndpoint,
                                OtherEndpoint.DeviceGuid,
                                thisLastSyncDt);
                            var otherDbLogResponse = SendReceive(client, thisDbLogRequest);
                            var remoteChangesLookup = await _localSyncData.PrepareRemoteLogForSyncing(otherDbLogResponse.Content);
                            DateTime newSyncDt = DateTime.UtcNow;
                            var disconnectRequest = MpStreamMessage.CreateDisconnectRequest(
                                ThisEndpoint, OtherEndpoint.DeviceGuid,
                                newSyncDt);
                            var disconnectResponse = SendReceive(client, disconnectRequest);

                            client.Close();

                            await _localSyncData.PerformSync(
                                remoteChangesLookup,
                                newSyncDt,
                                OtherEndpoint.DeviceGuid);
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
                    try {
                        var listener = ConnectSocket(@"192.168.43.209", 44381);
                        //var dbFileResponse = MpStreamMessage.CreateDbFileResponse(ThisEndpoint, string.Empty, _localSyncData.GetDbFileAsBase64());
                        //Send(listener, dbFileResponse);
                        //return;
                        var handshakeRequest = MpStreamMessage.CreateHandshakeRequest(ThisEndpoint);
                        var handshakeResponse = SendReceive(listener, handshakeRequest);
                        OtherEndpoint = MpDeviceEndpoint.Parse(handshakeResponse.Content);
                        var lastSyncDt = await _localSyncData.GetLastSyncForRemoteDevice(OtherEndpoint.DeviceGuid);
                        var dbLogRequest = MpStreamMessage.CreateDbLogRequest(
                            ThisEndpoint,
                            OtherEndpoint.DeviceGuid,
                            lastSyncDt);
                        var dbLogResponse = SendReceive(listener, dbLogRequest);
                        var remoteChangesLookup = await _localSyncData.PrepareRemoteLogForSyncing(dbLogResponse.Content);
                        var flipRequest = MpStreamMessage.CreateFlipRequest(
                            ThisEndpoint,
                            OtherEndpoint.DeviceGuid);

                        var odbLogRequest = SendReceive(listener, flipRequest);
                        var lastSyncForOther = DateTime.Parse(odbLogRequest.Content);
                        var dbLogQueryResultStr = await _localSyncData.GetLocalLogFromSyncDate(lastSyncForOther);
                        var thisdbLogResponse = MpStreamMessage.CreateDbLogResponse(ThisEndpoint, OtherEndpoint.DeviceGuid, dbLogQueryResultStr);
                        var disconnectRequest = SendReceive(listener, thisdbLogResponse);
                        var newSyncDt = DateTime.Parse(disconnectRequest.Content);
                        var disconnectResponse = MpStreamMessage.CreateDisconnectResponse(
                            ThisEndpoint,
                            OtherEndpoint.DeviceGuid,
                            newSyncDt);
                        Send(listener, disconnectResponse);
                        listener.Close();

                        await _localSyncData.PerformSync(
                            remoteChangesLookup,
                            newSyncDt,
                            OtherEndpoint.DeviceGuid);
                    }
                    catch(Exception ex) {
                        MpConsole.WriteTraceLine(@"Sync exception: ", ex);
                    }
                }
            });
        }

        private void LocalSync_OnSyncableChange(object sender, object e) {
            throw new NotImplementedException();
        }


        #region Network I/O
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
            var result = Send(s, smsg);
            if(result != null) {
                return result;
            }
            var responeStreamMsg = Receive(s);            
            return responeStreamMsg;
        }

        private MpStreamMessage Send(Socket s, MpStreamMessage smsg) {
            if (s == null) {
                return MpStreamMessage.CreateErrorMessage(ThisEndpoint, string.Empty, MpSyncMesageType.ErrorNotConnected, @"Socket is null");
            }
            if (!s.Connected) {
                return MpStreamMessage.CreateErrorMessage(ThisEndpoint, string.Empty, MpSyncMesageType.ErrorNotConnected, $"Cannot connect to socket: {s.ToString()}");
            }
            string smsgStr = smsg.SerializeDbObject();
            Byte[] bytesSent = Encoding.ASCII.GetBytes(smsgStr);

            MpConsole.WriteLine(@"Sending {0}: " + smsgStr, Enum.GetName(typeof(MpSyncMesageType),smsg.Header.MessageType));
            s.Send(bytesSent, bytesSent.Length, SocketFlags.None);
            MpConsole.WriteLine(@"Sent: {0} bytes", bytesSent.Length);
            return null;
        }
        private MpStreamMessage Receive(Socket s) {
            var response = ReceiveAll(s);
            
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

        private string ReceiveAll(Socket socket) {
            var buffer = new List<byte>();
            string response = string.Empty;

            while (socket.Available == 0) {
                //if (!SocketConnected(socket)) {
                //    throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, socket, @"Client not connected");
                //}
            }
            while (response == string.Empty || response.IndexOf(MpStreamMessage.EofToken) < 0) {
                while (socket.Available > 0) {
                    var currByte = new byte[1];
                    var byteCounter = socket.Receive(currByte, currByte.Length, SocketFlags.None);

                    if (byteCounter.Equals(1)) {
                        buffer.Add(currByte[0]);
                    }
                }
                response += Encoding.UTF8.GetString(buffer.ToArray());
                buffer.Clear();
            }
            return response;
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
        #endregion

        public void Dispose() {
            SessionManager.Dispose();
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
