using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Linq;
using System.Net.WebSockets;
using Org.BouncyCastle.Utilities.Encoders;

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
        private MpISync _localSync;
        private bool _isListener;
        private string _lastMsg = string.Empty;
        private string _lastSend, _lastReceive;

        private ObservableCollection<MpRemoteDevice> _remoteDevices = new ObservableCollection<MpRemoteDevice>();
        #endregion

        public event EventHandler<string> OnError;

        #region Properties
        public MpDeviceEndpoint ThisEndpoint { get; set; }

        public MpSessionManager SessionManager { get; set; }

        public bool IsRunning { get; set; } = false;
        #endregion

        #region Public Methods
        public void Init(MpISync localSync) {
            localSync.OnSyncableChange += LocalSync_OnSyncableChange;
            SessionManager = new MpSessionManager();
            Task.Run(async () => {
                _localSync = localSync;
                IsRunning = true;

                Socket listener = null;
                ClientWebSocket cws = null;

                while (IsRunning) {
                    while(listener != null) {
                        //occurs when client has already sync with an available listener
                        await Task.Delay(500);
                    }
                    _remoteDevices.Clear();
                    while (!_localSync.IsConnectedToNetwork()) {
                        await Task.Delay(10000);
                    }
                    ThisEndpoint = MpDeviceEndpointFactory.CreateEndpoint(localSync);

                    if (_localSync.IsConnectedToInternet()) {
                        //cws = await ConnectWebSocket();
                        //if(cws != null) {
                        //    var handshakeRequest = MpStreamMessage.CreateHandshakeRequest(ThisEndpoint);
                        //    await SendWebSocketAsync(cws, handshakeRequest);
                        //    var handshakeResponse = await ReceiveWebSocketAsync(cws);
                            
                        //}
                        if(!_localSync.IsWpf()) {
                            // for debugging disconnect all
                            await SessionManager.Disconnect(ThisEndpoint,true);
                        }
                        // check-in w/ webserver and add non-local endpoints
                        var webResponse = await SessionManager.Connect(ThisEndpoint);
                        //cws = await ConnectWebSocket();
                        //await SendWebSocketAsync(cws, MpStreamMessage.CreateHandshakeRequest(ThisEndpoint));

                        //if (!_localSync.IsWpf() &&
                        //    webResponse.Count == 1 &&
                        //    webResponse.Where(x => x.AccessToken == ThisEndpoint.AccessToken).FirstOrDefault() != null) {
                        //    // BUG when android creates socket listener cannot get wpf client to connect 
                        //    // so loop back until another device is connected to act as listner...
                        //    await SessionManager.Disconnect(ThisEndpoint);
                        //    await Task.Delay(10000);
                        //    continue;
                        //}
                        foreach (var rep in webResponse) {
                            if(rep.DeviceGuid == ThisEndpoint.DeviceGuid) {
                                continue;
                            }
                            //if(!ThisEndpoint.IsLocal(rep) || !rep.IsPrivateListening) {
                                //ignore ep's not on local network or not reporting listening (to avoid stupid Socket Exception)
                                //continue;
                            //}
                            try {
                                var rs = ConnectSocket(rep);
                                if (rs != null && listener == null) {
                                    listener = rs;
                                }
                                _remoteDevices.Add(new MpRemoteDevice(rs, rep));                                
                            }
                            catch(Exception ex) {
                                MpConsole.WriteTraceLine(@"Could not connect to listener: " + rep.ToString());
                                continue;
                            }
                        }
                    } else {
                        // no internet so scan local network
                        if (System.Diagnostics.Debugger.IsAttached) {
                            //...
                        } else {
                            // only do local ip sweep in release to avoid turning all the 
                            //exceptions off
                            listener = await MpPrivateEndpointDiscoveryHelper.Discover(
                                                ThisEndpoint.PrivateIp4Address,
                                                ThisEndpoint.PrivateConnectPortNum,
                                                localSync.IsWpf());
                        }
                    }

                    if (listener == null) {
                        Socket server = CreateLocalListener(ThisEndpoint);
                        while (true) {
                            Socket client = null;
                            try {
                                MpConsole.WriteLine("Waiting for connection...");
                                if (server != null) {
                                    client = server.Accept();
                                } else {
                                    var tcpl = new TcpListener(ThisEndpoint.PrivateConnectIPEndPoint);
                                    tcpl.Start();
                                    client = tcpl.AcceptSocket();
                                }
                                MpConsole.WriteLine("Connection made");

                                var handshakeRequest = ReceiveSocket(client);
                                var oep = MpDeviceEndpoint.Parse(handshakeRequest.Content);
                                foreach(var rd in _remoteDevices) {
                                    //check known clients for this ip where this may be a reconnect so replace info with new if ip match
                                    if(rd.RemoteEndpoint.PrivateIp4Address == oep.PrivateIp4Address) {
                                        int rdIdx = _remoteDevices.IndexOf(rd);
                                        _remoteDevices[rdIdx].RemoteEndpoint = oep;
                                        _remoteDevices[rdIdx].RemoteSocket = client;
                                    }
                                }

                                var handshakeResponse = MpStreamMessage.CreateHandshakeResponse(ThisEndpoint, oep.DeviceGuid);
                                var dbLogRequest = SendReceiveSocket(client, handshakeResponse);
                                var lastSyncForOther = DateTime.Parse(dbLogRequest.Content);
                                var dbLogQueryResultStr = await _localSync.GetLocalLogFromSyncDate(lastSyncForOther);
                                var dbLogResponse = MpStreamMessage.CreateDbLogResponse(ThisEndpoint, oep.DeviceGuid, dbLogQueryResultStr);

                                var flipRequest = SendReceiveSocket(client, dbLogResponse);
                                var thisLastSyncDt = _localSync.GetLastSyncForRemoteDevice(oep.DeviceGuid);
                                var thisDbLogRequest = MpStreamMessage.CreateDbLogRequest(
                                    ThisEndpoint,
                                    oep.DeviceGuid,
                                    thisLastSyncDt);
                                SendSocket(client, thisDbLogRequest);

                                Task.Run(async () => {
                                    while (client.Connected) {
                                        var dbLogResponse = ReceiveSocket(client);
                                        if(dbLogResponse.Header.FromGuid == ThisEndpoint.DeviceGuid) {
                                            continue;
                                        }
                                        var remoteChangesLookup = await _localSync.PrepareRemoteLogForSyncing(dbLogResponse.Content);
                                        await _localSync.PerformSync(
                                            remoteChangesLookup,
                                            oep.DeviceGuid);

                                        var rep = _remoteDevices.Where(x => x.RemoteEndpoint.DeviceGuid == oep.DeviceGuid).FirstOrDefault();
                                        if (rep == null) {
                                            _remoteDevices.Add(new MpRemoteDevice(client, oep));
                                        } else {
                                            var repIdx = _remoteDevices.IndexOf(rep);
                                            if (repIdx < 0) {
                                                throw new Exception("Unknown remote endpoint");
                                            }
                                        }
                                    }
                                    var cep = _remoteDevices.Where(x => x.RemoteEndpoint.DeviceGuid == oep.DeviceGuid).FirstOrDefault();
                                    if(cep != null) {
                                        _remoteDevices.Remove(cep);
                                        cep.RemoteSocket.Close();
                                    }              
                                });
                            }
                            catch (Exception ex) {
                                if (ex is MpSyncException) {
                                    client.Shutdown(SocketShutdown.Both);
                                    client.Dispose();
                                    client = null;
                                    MpConsole.WriteLine("Connection lost with client");
                                }
                            }
                        }
                    } else {
                        try {
                            var handshakeRequest = MpStreamMessage.CreateHandshakeRequest(ThisEndpoint);
                            var handshakeResponse = SendReceiveSocket(listener, handshakeRequest);
                            var lep = MpDeviceEndpoint.Parse(handshakeResponse.Content);
                            var lastSyncDt = _localSync.GetLastSyncForRemoteDevice(lep.DeviceGuid);

                            var dbLogRequest = MpStreamMessage.CreateDbLogRequest(
                                ThisEndpoint,
                                lep.DeviceGuid,
                                lastSyncDt);
                            var dbLogResponse = SendReceiveSocket(listener, dbLogRequest);
                            var remoteChangesLookup = await _localSync.PrepareRemoteLogForSyncing(dbLogResponse.Content);
                            var flipRequest = MpStreamMessage.CreateFlipRequest(
                                ThisEndpoint,
                                lep.DeviceGuid);

                            var odbLogRequest = SendReceiveSocket(listener, flipRequest);
                            var lastSyncForOther = DateTime.Parse(odbLogRequest.Content);
                            var dbLogQueryResultStr = await _localSync.GetLocalLogFromSyncDate(lastSyncForOther);
                            var thisdbLogResponse = MpStreamMessage.CreateDbLogResponse(ThisEndpoint, lep.DeviceGuid, dbLogQueryResultStr);
                            SendSocket(listener, thisdbLogResponse);

                            await _localSync.PerformSync(
                                remoteChangesLookup,
                                lep.DeviceGuid);                            

                            await Task.Run(async () => {
                                while (listener.Connected) {
                                    var dbLogResponse = ReceiveSocket(listener);
                                    if(dbLogResponse.Header.FromGuid == ThisEndpoint.DeviceGuid) {
                                        continue;
                                    }
                                    remoteChangesLookup = await _localSync.PrepareRemoteLogForSyncing(dbLogResponse.Content);
                                    await _localSync.PerformSync(
                                                remoteChangesLookup,
                                                lep.DeviceGuid);
                                }
                                listener.Close();
                                listener = null;
                            });
                        }
                        catch (Exception ex) {
                            MpConsole.WriteTraceLine(@"Sync exception: ", ex);
                        }
                    }
                }
            });
        }

        private void LocalSync_OnSyncableChange(object sender, object e) {
            Task.Run(async () => {
                if (e is string dboGuid) {
                    var llogs = await _localSync.GetDbObjectLogs(dboGuid, DateTime.MinValue);
                    if(llogs.Count == 0) {
                        return;
                    }
                    foreach (var rep in _remoteDevices) {
                        var lastSyncDt =  _localSync.GetLastSyncForRemoteDevice(rep.RemoteEndpoint.DeviceGuid);
                        var dbLogQueryResultStr = await _localSync.GetLocalLogFromSyncDate(lastSyncDt,rep.RemoteEndpoint.DeviceGuid);
                        if(!string.IsNullOrEmpty(dbLogQueryResultStr)) {
                            var thisdbLogResponse = MpStreamMessage.CreateDbLogResponse(ThisEndpoint, rep.RemoteEndpoint.DeviceGuid, dbLogQueryResultStr);
                            SendSocket(rep.RemoteSocket, thisdbLogResponse);
                        }
                    }
                }
            });
        }

        #region Network I/O

        #region Private Network I/O
        private Socket CreateLocalListener(MpDeviceEndpoint tep) {
            var listener = new Socket(tep.PrivateConnectIPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(tep.PrivateConnectIPEndPoint);
            listener.Listen(10);
            return listener;
        }

        private Socket ConnectSocket(string server, int port) {
            try {
                // Get host related information.
                var hostEntry = Dns.GetHostEntry(server);

                // Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
                // an exception that occurs when the host IP Address is not compatible with the address family
                // (typical in the IPv6 case).
                foreach (IPAddress address in hostEntry.AddressList) {
                    if(address.AddressFamily != AddressFamily.InterNetwork) {
                        continue;
                    }
                    try {
                        IPEndPoint ipe = new IPEndPoint(address, port);
                        Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        tempSocket.Connect(ipe);

                        if (tempSocket.Connected) {
                            return tempSocket;
                        }
                    }
                    catch(Exception ex) {
                        MpConsole.WriteTraceLine(@"Couldn't connect to endpoint: " + server + ":" + port);
                        MpConsole.WriteLine("With exception: " + ex);
                        continue;
                    }
                }
            }
            catch(Exception ex) {
                MpConsole.WriteTraceLine(@"Error connecting to listener {0}:{1} w/ exception: {2}", server, port.ToString(),ex.ToString());
            }
            return null;
        }

        private Socket ConnectSocket(MpDeviceEndpoint otherEndpoint) {
            try {
                Socket tempSocket = new Socket(otherEndpoint.PrivateConnectIPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                tempSocket.Connect(otherEndpoint.PrivateConnectIPEndPoint);

                if (tempSocket.Connected) {
                    return tempSocket;
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine(@"Couldn't connect to endpoint: " + otherEndpoint.ToString());
                MpConsole.WriteLine("With exception: " + ex);
            }
            return null;
        }

        private MpStreamMessage SendReceiveSocket(Socket s, MpStreamMessage smsg) {
            var result = SendSocket(s, smsg);
            if(result != null) {
                return result;
            }
            var responeStreamMsg = ReceiveSocket(s);            
            return responeStreamMsg;
        }

        private MpStreamMessage SendSocket(Socket s, MpStreamMessage smsg) {
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

        private MpStreamMessage ReceiveSocket(Socket s) {
            var response = ReceiveAllSocket(s);
            
            if (!response.Contains(MpStreamMessage.EofToken)) {
                if(string.IsNullOrEmpty(response)) {
                    return MpStreamMessage.CreateErrorMessage(ThisEndpoint, string.Empty, MpSyncMesageType.ErrorNotConnected, "Lost connection");
                }
                return MpStreamMessage.CreateErrorMessage(ThisEndpoint, string.Empty, MpSyncMesageType.ErrorInvalidData, $"No eof token received for msg: {response}");
            } else {
                response = response.Replace(MpStreamMessage.EofToken, string.Empty);
            }
            var smsg = MpStreamMessage.Parse(response);
            MpConsole.WriteLine(@"Received {0}: " + response, Enum.GetName(typeof(MpSyncMesageType), smsg.Header.MessageType));
            return smsg;
        }

        private string ReceiveAllSocket(Socket socket) {
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

        private bool IsConnectedSocket(Socket s) {
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

        #region Public Network I/O

        private static async Task DoClientWebSocket() {
            using (ClientWebSocket ws = new ClientWebSocket()) {
                Uri serverUri = new Uri("wss://echo.websocket.org/");

                //Implementation of timeout of 5000 ms
                var source = new CancellationTokenSource();
                source.CancelAfter(5000);

                await ws.ConnectAsync(serverUri, source.Token);
                var iterationNo = 0;
                // restricted to 5 iteration only
                while (ws.State == WebSocketState.Open && iterationNo++ < 5) {
                    string msg = "hello0123456789123456789123456789123456789123456789123456789";
                    ArraySegment<byte> bytesToSend =
                                new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
                    await ws.SendAsync(bytesToSend, WebSocketMessageType.Text,
                                         true, source.Token);
                    //Receive buffer
                    var receiveBuffer = new byte[200];
                    //Multipacket response
                    var offset = 0;
                    var dataPerPacket = 10; //Just for example
                    while (true) {
                        ArraySegment<byte> bytesReceived =
                                  new ArraySegment<byte>(receiveBuffer, offset, dataPerPacket);
                        WebSocketReceiveResult result = await ws.ReceiveAsync(bytesReceived,
                                                                      source.Token);
                        //Partial data received
                        Console.WriteLine("Data:{0}",
                                         Encoding.UTF8.GetString(receiveBuffer, offset,
                                                                      result.Count));
                        offset += result.Count;
                        if (result.EndOfMessage)
                            break;
                    }
                    Console.WriteLine("Complete response: {0}",
                                        Encoding.UTF8.GetString(receiveBuffer, 0,
                                                                    offset));
                }
            }
        }


        public async Task<ClientWebSocket> ConnectWebSocket() {
            var socket = new ClientWebSocket();
            try {
                await socket.ConnectAsync(new Uri(@"ws://monkeypaste.com:8080"), CancellationToken.None);

                
                //await Receive(socket);

                return socket;

            }
            catch (Exception ex) {
                Console.WriteLine($"ERROR - {ex.Message}");
                return null;
            }
        }

        public async Task SendWebSocketAsync(ClientWebSocket cws, MpStreamMessage smsg) {
            var bytes = Encoding.ASCII.GetBytes(smsg.SerializeDbObject());
            var buffer = new ArraySegment<Byte>(bytes, 0, bytes.Length);

            await cws.SendAsync(
                buffer, 
                WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task<MpStreamMessage> ReceiveWebSocketAsync(ClientWebSocket ws) {
            var buffer = new ArraySegment<byte>(new Byte[8192]);
            WebSocketReceiveResult result = null;
            using (var ms = new MemoryStream()) {
                do {
                    result = await ws.ReceiveAsync(buffer, CancellationToken.None);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);

                using (var reader = new StreamReader(ms, Encoding.ASCII)) {
                    var response = reader.ReadToEnd();
                    MpConsole.WriteLine(@"Received from server: " + response);
                    return MpStreamMessage.Parse(response);
                }
            }
        }
        #endregion

        #endregion
        public void Dispose() {
            if(ThisEndpoint == null) {
                return;
            }
            SessionManager.Disconnect(ThisEndpoint);
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
