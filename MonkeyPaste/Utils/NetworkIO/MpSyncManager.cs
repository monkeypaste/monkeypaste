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
        public MpSyncMesageType ErrorType { get; set; }
        public MpRemoteDevice RemoteDevice { get; set; }
        public MpSyncException(MpSyncMesageType errorType, MpRemoteDevice rd) : base() {
            ErrorType = errorType;
            RemoteDevice = rd;
        }
    }
    #endregion

    public class MpSyncManager : IDisposable {
        #region Singleton
        private static readonly Lazy<MpSyncManager> _Lazy = new Lazy<MpSyncManager>(() => new MpSyncManager());
        public static MpSyncManager Instance { get { return _Lazy.Value; } }
        #endregion

        #region Private Variables
        private MpISync _localSync;

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
                        //if(_localSync.IsWpf()) {
                        //    // for debugging disconnect all
                        //    await SessionManager.Disconnect(ThisEndpoint,true);
                        //}
                        // check-in w/ webserver and add non-local endpoints
                        if(cws == null) {
                            cws = await ConnectWebSocket();
                        } else {
                            _remoteDevices.Clear();
                        }
                        if (cws != null) {
                            var webConnectResponse = await SendReceiveWebSocket(cws, MpStreamMessage.CreateWebDeviceRequest(ThisEndpoint));
                            var availableDeviceList = ProcessWebConnectResponse(webConnectResponse.Content);

                            foreach (var rep in availableDeviceList) {
                                if (rep.DeviceGuid == ThisEndpoint.DeviceGuid) {
                                    continue;
                                }

                                try {
                                    object deviceSocket = cws;
                                    if (ThisEndpoint.IsLocal(rep)) {
                                        var rs = ConnectSocket(rep);
                                        if (rs != null) {
                                            if (listener == null) {
                                                listener = rs;
                                            }
                                            deviceSocket = rs;
                                        }
                                    }
                                    _remoteDevices.Add(new MpRemoteDevice(deviceSocket, rep));
                                }
                                catch (Exception ex) {
                                    MpConsole.WriteTraceLine(@"Could not connect to listener: " + rep.ToString() + " w/ exception: "+ex);
                                    continue;
                                }
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
                                                ThisEndpoint.PrimaryPrivateIp4Address,
                                                ThisEndpoint.PrivatePortNum,
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
                                    var tcpl = new TcpListener(ThisEndpoint.PrivateIPEndPoint);
                                    tcpl.Start();
                                    client = tcpl.AcceptSocket();
                                }
                                MpConsole.WriteLine("Connection made");

                                var handshakeRequest = ReceiveSocket(client);
                                var oep = MpDeviceEndpoint.Parse(handshakeRequest.Content);
                                foreach(var rd in _remoteDevices) {
                                    //check known clients for this ip where this may be a reconnect so replace info with new if ip match
                                    if(rd.RemoteEndpoint.PrimaryPrivateIp4Address == oep.PrimaryPrivateIp4Address) {
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
                                var thisLastSyncDt = await _localSync.GetLastSyncForRemoteDevice(oep.DeviceGuid);
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
                                        _localSync.PerformSync(
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
                                        (cep.RemoteSocket as Socket).Close();
                                    }              
                                });
                            }
                            catch (Exception ex) {
                                if (ex is MpSyncException se) {
                                    if (_remoteDevices.Contains(se.RemoteDevice)) {
                                        _remoteDevices.Remove(se.RemoteDevice);
                                    }
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
                            var lastSyncDt = await _localSync.GetLastSyncForRemoteDevice(lep.DeviceGuid);

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
                            if(ex is MpSyncException se) {
                                if(_remoteDevices.Contains(se.RemoteDevice)) {
                                    _remoteDevices.Remove(se.RemoteDevice);
                                }
                            }
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
                        var lastSyncDt = await _localSync.GetLastSyncForRemoteDevice(rep.RemoteEndpoint.DeviceGuid);
                        var dbLogQueryResultStr = await _localSync.GetLocalLogFromSyncDate(lastSyncDt,rep.RemoteEndpoint.DeviceGuid);
                        if(!string.IsNullOrEmpty(dbLogQueryResultStr)) {
                            var thisdbLogResponse = MpStreamMessage.CreateDbLogResponse(ThisEndpoint, rep.RemoteEndpoint.DeviceGuid, dbLogQueryResultStr);
                            Send(rep, thisdbLogResponse);
                        }
                    }
                }
            });
        }


        #region Network I/O

        private async Task Send(MpRemoteDevice rep,MpStreamMessage msg) {
            if(rep.RemoteSocket == null) {
                MpConsole.WriteTraceLine($"Error socket is null");
                return;
            }
            if (rep.RemoteSocket is Socket s) {
                SendSocket(s, msg);
            } else if(rep.RemoteSocket is ClientWebSocket cws) {
                await SendWebSocketAsync(cws, msg);
            }
        }

        private async Task<MpStreamMessage> Receive(MpRemoteDevice rep) {
            if (rep.RemoteSocket == null) {
                throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, rep);
            }
            if (rep.RemoteSocket is Socket s) {
                return ReceiveSocket(s);
            } else if (rep.RemoteSocket is ClientWebSocket cws) {
                var response = await ReceiveWebSocketAsync(cws);
                return response;
            }

            throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, rep);
        }

        private async Task<MpStreamMessage> SendReceive(MpRemoteDevice rep, MpStreamMessage msg) {
            if (rep.RemoteSocket == null) {
                throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, rep);
            }
            if (rep.RemoteSocket is Socket s) {
                var response = SendReceiveSocket(s,msg);
                return response;
            } else if (rep.RemoteSocket is ClientWebSocket cws) {
                var response = await ReceiveWebSocketAsync(cws);
                return response;
            }

            throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, rep);
        }

        #region Private Network I/O 
        private Socket CreateLocalListener(MpDeviceEndpoint tep) {
            var listener = new Socket(tep.PrivateIPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(tep.PrivateIPEndPoint);
            listener.Listen(10);
            return listener;
        }

        private Socket ConnectSocket(MpDeviceEndpoint otherEndpoint) {
            try {
                Socket tempSocket = new Socket(otherEndpoint.PrivateIPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                tempSocket.Connect(otherEndpoint.PrivateIPEndPoint);

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
            if (result != null) {
                return result;
            }
            var responeStreamMsg = ReceiveSocket(s);
            return responeStreamMsg;
        }

        private MpStreamMessage SendSocket(Socket s, MpStreamMessage smsg) {
            if (s == null) {
                var rd = _remoteDevices.Where(x => x.RemoteSocket == (object)s).FirstOrDefault();
                throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, rd);
            }
            if (!s.Connected) {
                var rd = _remoteDevices.Where(x => x.RemoteSocket == (object)s).FirstOrDefault();
                throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, rd);
            }
            string smsgStr = smsg.SerializeDbObject();
            Byte[] bytesSent = Encoding.ASCII.GetBytes(smsgStr);

            MpConsole.WriteLine(@"Sending {0}: " + smsgStr, Enum.GetName(typeof(MpSyncMesageType), smsg.Header.MessageType));
            try {
                s.Send(bytesSent, bytesSent.Length, SocketFlags.None);
            }
            catch (Exception ex) {
                if (ex is MpSyncException se) {
                    throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, null);
                }
            }
            MpConsole.WriteLine(@"Sent: {0} bytes", bytesSent.Length);
            return null;
        }

        private MpStreamMessage ReceiveSocket(Socket s) {
            var response = ReceiveAllSocket(s);

            if (!response.Contains(MpStreamMessage.EofToken)) {
                var rd = _remoteDevices.Where(x => x.RemoteSocket == (object)s).FirstOrDefault();
                throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, rd);
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
                    try {
                        var currByte = new byte[1];
                        var byteCounter = socket.Receive(currByte, currByte.Length, SocketFlags.None);

                        if (byteCounter.Equals(1)) {
                            buffer.Add(currByte[0]);
                        }
                    }
                    catch(Exception ex) {
                        if(ex is MpSyncException se) {
                            throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, null);
                        }
                    }
                }
                response += Encoding.ASCII.GetString(buffer.ToArray());
                buffer.Clear();
            }
            return response;
        }
        #endregion

        #region Public Network I/O

        private List<MpDeviceEndpoint> ProcessWebConnectResponse(string response) {
            var repl = new List<MpDeviceEndpoint>();
            if (string.IsNullOrEmpty(response)) {
                return repl;
            }
            var rpl = response.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < rpl.Length; i++) {
                if (i + 6 > rpl.Length) {
                    MpConsole.WriteTraceLine(@"Malformed remote device response from server, ignoring");
                    break;
                }
                //format: "$oip&$oprivip&$oportnum&$oat&$oldt&$odg&";
                var ep = new MpDeviceEndpoint();
                ep.PublicIp4Address = rpl[i];
                ep.PrivateIp4Addresses = new ObservableCollection<string>(rpl[++i].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                ep.PublicPortNum = ep.PrivatePortNum = Convert.ToInt32(rpl[++i]);
                ep.AccessToken = rpl[++i];
                ep.ConnectDateTime = DateTime.Parse(rpl[++i]);
                ep.DeviceGuid = rpl[++i];
                ep.PrimaryPrivateIp4AddressIdx = Convert.ToInt32(rpl[++i]);
                repl.Add(ep);
            }
            return repl;
        }

        public async Task<ClientWebSocket> ConnectWebSocket() {
            var serverIp = MpHelpers.Instance.GetIpForDomain("www.monkeypaste.com");
            var socket = new ClientWebSocket();
            try {
                await socket.ConnectAsync(new Uri(@"ws://206.72.205.68:8080"), CancellationToken.None);
            }
            catch(WebSocketException wsex) {
                Console.WriteLine($"ERROR - {wsex.Message}");
                return null;
            }
            catch (Exception ex) {
                Console.WriteLine($"ERROR - {ex.Message}");
                return null;
            }

            return socket;
        }

        public async Task SendWebSocketAsync(ClientWebSocket cws, MpStreamMessage smsg) {
            var bytes = Encoding.ASCII.GetBytes(smsg.SerializeDbObject());
            var buffer = new ArraySegment<Byte>(bytes, 0, bytes.Length);

            await cws.SendAsync(
                buffer,
                WebSocketMessageType.Text, true, CancellationToken.None);
        }


        public async Task<MpStreamMessage> ReceiveWebSocketAsync(ClientWebSocket ws) {
            var response = string.Empty;
            var buffer = new ArraySegment<byte>(new Byte[8192]);
            WebSocketReceiveResult result = null;
            while (!response.Contains(MpStreamMessage.EofToken)) {
                using (var ms = new MemoryStream()) {                
                    do {
                        result = await ws.ReceiveAsync(buffer, CancellationToken.None);
                        ms.Write(buffer.Array, buffer.Offset, result.Count);
                    }
                    while (!result.EndOfMessage);

                    ms.Seek(0, SeekOrigin.Begin);

                    using (var reader = new StreamReader(ms, Encoding.ASCII)) {
                        response += reader.ReadToEnd();
                        MpConsole.WriteLine(@"Received from server: " + response);

                    }
                }
            }
            return MpStreamMessage.Parse(response);
        }

        private async Task<MpStreamMessage> SendReceiveWebSocket(ClientWebSocket cws, MpStreamMessage smsg) {
            await SendWebSocketAsync(cws, smsg);
            var responeStreamMsg = await ReceiveWebSocketAsync(cws);
            return responeStreamMsg;
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
