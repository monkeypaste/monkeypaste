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
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;

namespace MonkeyPaste {

    public static class MpSyncManager {

        #region Private Variables

        private static CancellationTokenSource _cts;
        private static CancellationToken _ct;
        #endregion

        //public static event EventHandler<string> OnError;

        #region Properties
        public static string[] DbTableSyncOrder { get; private set; } = new string[] {
            nameof(MpDbImage),
            nameof(MpIcon),
            nameof(MpUserDevice),
            nameof(MpUrl),
            nameof(MpApp),
            nameof(MpSource),
            nameof(MpCopyItem),
            nameof(MpTag),
            nameof(MpTextTemplate),
            nameof(MpCopyItemTag)
        };

        public static MpDeviceEndpoint ThisEndpoint { get; set; }

        public static ObservableCollection<MpRemoteDevice> ConnectedDevices { get; set; }

        public static MpRemoteDevice SelectedRemoteDevice {
            get {
                if (ConnectedDevices == null) {
                    return null;
                }
                foreach (var rd in ConnectedDevices) {
                    if (rd.IsSelected) {
                        return rd;
                    }
                }
                return null;
            }
        }

        public static MpSessionManager SessionManager { get; set; }

        public static bool IsRunning { get; set; } = false;
        #endregion

        #region Public Methods
        public static void Init() {
            _cts = new CancellationTokenSource();
            _ct = _cts.Token;
            MpSyncHelpers.OnSyncableChange += LocalSync_OnSyncableChange;

            ConnectedDevices = MpSyncHelpers.GetRemoteDevices();

            SessionManager = new MpSessionManager();

            IsRunning = true;

            ThisEndpoint = MpDeviceEndpointFactory.CreateEndpoint();

            Task.Run(async () => {
                Socket listener = null;
                ClientWebSocket cws = null;

                while (IsRunning) {
                    //Task.Run(PerformRemoteSyncLoop);
                    while(listener != null) {
                        //occurs when client has already sync with an available listener
                        await Task.Delay(500);
                    }
                    ConnectedDevices.Clear();
                    while (!MpSyncHelpers.IsConnectedToNetwork()) {
                        await Task.Delay(10000);
                    }
                    ThisEndpoint = MpDeviceEndpointFactory.CreateEndpoint();

                    if (MpSyncHelpers.IsConnectedToInternet()) {
                        
                        //if(_localSync.IsWpf()) {
                        //    // for debugging disconnect all
                        //    await SessionManager.Disconnect(ThisEndpoint,true);
                        //}
                        // check-in w/ webserver and add non-local endpoints
                        if(cws == null) {
                            cws = await ConnectWebSocket();
                        } else {
                            ConnectedDevices.Clear();
                        }
                        if (cws != null) {
                            var wdr = await MpStreamMessage.CreateWebDeviceRequest(ThisEndpoint);
                            var webConnectResponse = await SendReceiveWebSocketAsync(cws, wdr);
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
                                    ConnectedDevices.Add(new MpRemoteDevice(deviceSocket, rep));
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
                                                MpSyncHelpers.IsWpf());
                        }
                    }

                    if (listener == null) {
                        listener = CreateLocalListener(ThisEndpoint);
                        while (true) {
                            Socket client = null;
                            try {
                                MpConsole.WriteLine("Waiting for connection...");
                                client = listener.Accept();

                                MpConsole.WriteLine("Connection made");

                                var handshakeRequest = await ReceiveSocket(client);
                                var oep = await MpDeviceEndpoint.Parse(handshakeRequest.Content);
                                foreach(var rd in ConnectedDevices) {
                                    //check known clients for this ip where this may be a reconnect so replace info with new if ip match
                                    if(rd.RemoteEndpoint.PrimaryPrivateIp4Address == oep.PrimaryPrivateIp4Address) {
                                        int rdIdx = ConnectedDevices.IndexOf(rd);
                                        ConnectedDevices[rdIdx].RemoteEndpoint = oep;
                                        ConnectedDevices[rdIdx].RemoteSocket = client;
                                    }
                                }

                                var handshakeResponse = await MpStreamMessage.CreateHandshakeResponse(ThisEndpoint, oep.DeviceGuid);
                                var dbLogRequest = await SendReceiveSocket(client, handshakeResponse);
                                var lastSyncForOther = DateTime.Parse(dbLogRequest.Content);
                                var dbLogQueryResultStr = await MpSyncHelpers.GetLocalLogFromSyncDate(lastSyncForOther);
                                var dbLogResponse = await MpStreamMessage.CreateDbLogResponse(ThisEndpoint, oep.DeviceGuid, dbLogQueryResultStr);

                                var flipRequest = await SendReceiveSocket(client, dbLogResponse);
                                var thisLastSyncDt = await MpSyncHelpers.GetLastSyncForRemoteDevice(oep.DeviceGuid);
                                var thisDbLogRequest = await MpStreamMessage.CreateDbLogRequest(
                                    ThisEndpoint,
                                    oep.DeviceGuid,
                                    thisLastSyncDt);
                                await SendSocket (client, thisDbLogRequest);

                                //Task.Run(async () => {
                                    while (IsConnected(client,false)) {
                                        try {
                                            var postSyncDbLogResponse = await ReceiveSocket(client);
                                            if (postSyncDbLogResponse.Header.FromGuid == ThisEndpoint.DeviceGuid) {
                                                continue;
                                            }
                                            var remoteChangesLookup = await MpSyncHelpers.PrepareRemoteLogForSyncing(postSyncDbLogResponse.Content);
                                            await MpSyncHelpers.PerformSync(remoteChangesLookup,oep.DeviceGuid);

                                            var rep = ConnectedDevices.Where(x => x.RemoteEndpoint.DeviceGuid == oep.DeviceGuid).FirstOrDefault();
                                            if (rep == null) {
                                                ConnectedDevices.Add(new MpRemoteDevice(client, oep));
                                            } else {
                                                var repIdx = ConnectedDevices.IndexOf(rep);
                                                if (repIdx < 0) {
                                                    throw new Exception("Unknown remote endpoint");
                                                }
                                            }
                                        }
                                        catch (Exception ex) {
                                            client.Shutdown(SocketShutdown.Both);
                                            client.Close();
                                            client.Dispose();
                                            client = null;
                                            MpConsole.WriteTraceLine(@"Sync socket exception: ", ex);
                                            break;
                                        }
                                    }
                                    var cep = ConnectedDevices.Where(x => x.RemoteEndpoint.DeviceGuid == oep.DeviceGuid).FirstOrDefault();
                                    if(cep != null) {
                                        ConnectedDevices.Remove(cep);
                                        (cep.RemoteSocket as Socket).Close();
                                    }              
                                //},_ct);
                            }
                            catch (Exception ex) {
                                client.Shutdown(SocketShutdown.Both);
                                client.Close();
                                client.Dispose();
                                client = null;
                                MpConsole.WriteTraceLine(@"Sync socket exception: ", ex);
                                break;
                            }
                        }
                    } else {
                        try {
                            var handshakeRequest = await MpStreamMessage.CreateHandshakeRequest(ThisEndpoint);
                            var handshakeResponse = await SendReceiveSocket(listener, handshakeRequest);
                            var lep = await MpDeviceEndpoint.Parse(handshakeResponse.Content);
                            var lastSyncDt = await MpSyncHelpers.GetLastSyncForRemoteDevice(lep.DeviceGuid);

                            var dbLogRequest = await MpStreamMessage.CreateDbLogRequest(
                                ThisEndpoint,
                                lep.DeviceGuid,
                                lastSyncDt);
                            var dbLogResponse = await SendReceiveSocket(listener, dbLogRequest);
                            var remoteChangesLookup = await MpSyncHelpers.PrepareRemoteLogForSyncing(dbLogResponse.Content);
                            var flipRequest = await MpStreamMessage.CreateFlipRequest(
                                ThisEndpoint,
                                lep.DeviceGuid);

                            var odbLogRequest = await SendReceiveSocket(listener, flipRequest);
                            var lastSyncForOther = DateTime.Parse(odbLogRequest.Content);
                            var dbLogQueryResultStr = await MpSyncHelpers.GetLocalLogFromSyncDate(lastSyncForOther);
                            var thisdbLogResponse = await MpStreamMessage .CreateDbLogResponse(ThisEndpoint, lep.DeviceGuid, dbLogQueryResultStr);
                            await SendSocket(listener, thisdbLogResponse);

                            await MpSyncHelpers.PerformSync(
                                remoteChangesLookup,
                                lep.DeviceGuid);                            

                            await Task.Run(async () => {
                                while (IsConnected(listener,false)) {
                                    try {
                                        var dbLogResponse = await ReceiveSocket(listener);
                                        if (dbLogResponse == null) {
                                            //listener is no longer connected or an error is on the socket so reiterate to establish current rd list
                                            break;
                                        }
                                        if (dbLogResponse.Header.FromGuid == ThisEndpoint.DeviceGuid) {
                                            continue;
                                        }
                                        remoteChangesLookup = await MpSyncHelpers.PrepareRemoteLogForSyncing(dbLogResponse.Content);
                                        await MpSyncHelpers.PerformSync(
                                                    remoteChangesLookup,
                                                    lep.DeviceGuid);
                                    }
                                    catch (Exception ex) {
                                        listener = null;
                                        MpConsole.WriteTraceLine(@"Sync socket exception: ", ex);
                                        break;
                                    }
                                }
                                listener.Shutdown(SocketShutdown.Both);
                                listener.Close();
                                listener.Dispose();
                                listener = null;
                            },_ct);
                        }
                        catch (Exception ex) {
                            listener.Shutdown(SocketShutdown.Both);
                            listener.Close();
                            listener.Dispose();
                            listener = null;
                            MpConsole.WriteTraceLine(@"Sync socket exception: ", ex);
                        }
                    }
                }
            });
        }

        public static async Task DisposeAsync() {
            if(ThisEndpoint == null) {
                return;
            }
            await SessionManager.Disconnect(ThisEndpoint);
        }
        #endregion

        #region Private Methods
        private static void LocalSync_OnSyncableChange(object sender, object e) {
            if (sender == null) {
                return;
            }
            string dboTypeStr = sender.GetType().ToString();
            if(dboTypeStr.EndsWith(".MpTag") || dboTypeStr.EndsWith(".MpCopyItemTag")) {
                Task.Run(async () => {
                    if (e is string dboGuid) {
                        //var llogs = await _localSync.GetDbObjectLogs(dboGuid, DateTime.MinValue);
                        //if (llogs.Count == 0) {
                        //    return;
                        //}
                        var disconnectedDevices = new List<MpRemoteDevice>();
                        foreach (var rep in ConnectedDevices) {
                            var lastSyncDt = await MpSyncHelpers.GetLastSyncForRemoteDevice(rep.RemoteEndpoint.DeviceGuid);
                            var dbLogQueryResultStr = await MpSyncHelpers.GetLocalLogFromSyncDate(lastSyncDt, rep.RemoteEndpoint.DeviceGuid);
                            if (!string.IsNullOrEmpty(dbLogQueryResultStr)) {
                                var thisdbLogResponse = await MpStreamMessage .CreateDbLogResponse(ThisEndpoint, rep.RemoteEndpoint.DeviceGuid, dbLogQueryResultStr);
                                try {
                                    await Send(rep, thisdbLogResponse);
                                }
                                catch (Exception) {
                                    disconnectedDevices.Add(rep);
                                    continue;
                                }
                            }
                        }
                        if (disconnectedDevices.Count > 0) {
                            _cts.Cancel();
                        }
                    }
                });
            }
        }


        #region Network I/O

        #region Generic socket i/o
        private static async Task Send(MpRemoteDevice rep, MpStreamMessage msg) {
            if (rep.RemoteSocket == null) {
                MpConsole.WriteTraceLine($"Error socket is null");
                return;
            }
            if (rep.RemoteSocket is Socket s) {
                await SendSocket(s, msg);
            } else if (rep.RemoteSocket is ClientWebSocket cws) {
                await SendWebSocketAsync(cws, msg);
            }
        }

        private static async Task<MpStreamMessage> Receive(MpRemoteDevice rep) {
            if (rep.RemoteSocket == null) {
                throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, rep);
            }
            if (rep.RemoteSocket is Socket s) {
                return await ReceiveSocket(s);
            } else if (rep.RemoteSocket is ClientWebSocket cws) {
                var response = await ReceiveWebSocketAsync(cws);
                return response;
            }

            throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, rep);
        }

        private static async Task<MpStreamMessage> SendReceive(MpRemoteDevice rep, MpStreamMessage msg) {
            if (rep.RemoteSocket == null) {
                throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, rep);
            }
            if (rep.RemoteSocket is Socket s) {
                var response = await SendReceiveSocket(s, msg);
                return response;
            } else if (rep.RemoteSocket is ClientWebSocket cws) {
                var response = await ReceiveWebSocketAsync(cws);
                return response;
            }

            throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, rep);
        }
        #endregion

        #region TCP Sockets
        private static Socket CreateLocalListener(MpDeviceEndpoint tep) {
            var listener = new Socket(tep.PrivateIPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(tep.PrivateIPEndPoint);
            listener.Listen(10);
            return listener;
        }

        private static Socket ConnectSocket(MpDeviceEndpoint otherEndpoint) {
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

        private static async Task<MpStreamMessage> SendReceiveSocket(Socket s, MpStreamMessage smsg) {
            var result = await SendSocket(s, smsg);
            if (result != null) {
                return result;
            }
            var responeStreamMsg = await ReceiveSocket(s);
            return responeStreamMsg;
        }

        private static async Task<MpStreamMessage> SendSocket(Socket s, MpStreamMessage smsg) {
            if (!IsConnected(s,true)) {
                var rd = ConnectedDevices.Where(x => x.RemoteSocket == (object)s).FirstOrDefault();
                throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, rd);
            }
            string smsgStr = await smsg.SerializeDbObjectAsync();
            Byte[] bytesSent = Encoding.ASCII.GetBytes(smsgStr);

            MpConsole.WriteLine(string.Format(@"{0} Sending {1}: {2}", DateTime.Now.ToString(), Enum.GetName(typeof(MpSyncMesageType), smsg.Header.MessageType), smsgStr));
            try {
                s.Send(bytesSent, bytesSent.Length, SocketFlags.None);
            }
            catch (Exception ex) {
                var rd = ConnectedDevices.Where(x => x.RemoteSocket == (object)s).FirstOrDefault();
                throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, rd, ex);
            }

            if(smsg.Header.MessageType == MpSyncMesageType.DbLogResponse) {
               await MpSyncHelpers.UpdateSyncHistory(smsg.Header.ToGuid, DateTime.UtcNow);
            }

            MpConsole.WriteLine(@"{0} Sent: {1} bytes", DateTime.Now.ToString(), bytesSent.Length);
            return null;
        }

        private static async Task<MpStreamMessage> ReceiveSocket(Socket s) {
            var response = await ReceiveAllSocket(s);

            if (!response.Contains(MpStreamMessage.EofToken)) {
                var rd = ConnectedDevices.Where(x => x.RemoteSocket == (object)s).FirstOrDefault();
                throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, rd);
            } else {
                response = response.Replace(MpStreamMessage.EofToken, string.Empty);
            }
            var smsg = await MpStreamMessage.Parse(response);
            MpConsole.WriteLine(string.Format(@"{0} Received {1}: {2}", DateTime.Now.ToString(), Enum.GetName(typeof(MpSyncMesageType), smsg.Header.MessageType), response));
            return smsg;
        }

        private static async Task<string> ReceiveAllSocket(Socket socket) {
            var buffer = new List<byte>();
            string response = string.Empty;
            bool isConnected = IsConnected(socket,false);

            while (socket.Available == 0) {
                if (!isConnected) {
                    var rd = ConnectedDevices.Where(x => x.RemoteSocket == (object)socket).FirstOrDefault();
                    throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, rd);
                }
                await Task.Delay(100);
                isConnected = IsConnected(socket,false);
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
                    catch (Exception ex) {
                        var rd = ConnectedDevices.Where(x => x.RemoteSocket == (object)socket).FirstOrDefault();
                        throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, rd, ex);
                    }
                }
                response += Encoding.ASCII.GetString(buffer.ToArray());
                buffer.Clear();
            }
            return response;
        }

        private static bool IsConnected(Socket s, bool isWrite) {
            return true;
            //int pt = 500000; //500000 microseconds is .5 seconds
            //if (s == null) {
            //    return false;
            //}
            ////bool isConnected = true;
            //if (!s.Connected) {
            //    return false;
            //}
            //if (isWrite && !s.Poll(pt, SelectMode.SelectWrite)) {
            //    Console.WriteLine("This Socket is not writable.");
            //    //isConnected = false;
            //    return false;
            //}
            //if (!isWrite && !s.Poll(pt, SelectMode.SelectRead)) {
            //    //Console.WriteLine("This Socket is not readable.");
            //    return false;
            //}
            ////if (s.Poll(pt, SelectMode.SelectError)) {
            ////    //Console.WriteLine("This Socket has an error.");
            ////    return false;
            ////}
            //return true;
        }
        #endregion

        #region Web Sockets

        private static List<MpDeviceEndpoint> ProcessWebConnectResponse(string response) {
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

        private static async Task<ClientWebSocket> ConnectWebSocket() {
            var serverIp = MpNetworkHelpers.GetIpForDomain("www.monkeypaste.com");
            var socket = new ClientWebSocket();
            try {
                await socket.ConnectAsync(new Uri(string.Format(@"ws://{0}:8080",serverIp)), CancellationToken.None);
            }
            catch (WebSocketException wsex) {
                Console.WriteLine($"ERROR - {wsex.Message}");
                return null;
            }
            catch (Exception ex) {
                Console.WriteLine($"ERROR - {ex.Message}");
                return null;
            }

            return socket;
        }

        private static async Task SendWebSocketAsync(ClientWebSocket cws, MpStreamMessage smsg) {
            string smsgStr = await smsg.SerializeDbObjectAsync();
            var bytes = Encoding.ASCII.GetBytes(smsgStr);
            var buffer = new ArraySegment<Byte>(bytes, 0, bytes.Length);

            if (!IsConnected(cws)) {
                var rd = ConnectedDevices.Where(x => x.RemoteSocket == (object)cws).FirstOrDefault();
                throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, rd);
            }
            await cws.SendAsync(
                buffer,
                WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private static async Task<MpStreamMessage> ReceiveWebSocketAsync(ClientWebSocket cws) {
            var response = string.Empty;
            var buffer = new ArraySegment<byte>(new Byte[8192]);
            WebSocketReceiveResult result = null;
            while (!response.Contains(MpStreamMessage.EofToken)) {
                //Task.Run(async () => {
                //    while (IsConnected(cws)) {
                //        await Task.Delay(100);
                //    }
                //    _cts.Cancel();
                //}, _ct);
                using (var ms = new MemoryStream()) {
                    do {
                        result = await cws.ReceiveAsync(buffer, _ct);
                        ms.Write(buffer.Array, buffer.Offset, result.Count);
                    }
                    while (!result.EndOfMessage);

                    if (!_ct.IsCancellationRequested) {
                        ms.Seek(0, SeekOrigin.Begin);

                        using (var reader = new StreamReader(ms, Encoding.ASCII)) {
                            response += reader.ReadToEnd();
                            MpConsole.WriteLine(@"Received from server: " + response);
                        }
                    } else {
                        //this will cancel the check connection thread if no connection problems
                        _cts.Cancel();
                        return null;
                    }

                }
            }
            var respMsg = await MpStreamMessage.Parse(response);
            return respMsg;
        }

        private static async Task<MpStreamMessage> SendReceiveWebSocketAsync(ClientWebSocket cws, MpStreamMessage smsg) {
            await SendWebSocketAsync(cws, smsg);
            var responseStreamMsg = await ReceiveWebSocketAsync(cws);
            if (responseStreamMsg == null) {
                //means disconnected/error with web socket, throw sync exception to reiterate main sync loop
                var rd = ConnectedDevices.Where(x => x.RemoteSocket == (object)cws).FirstOrDefault();
                throw new MpSyncException(MpSyncMesageType.ErrorNotConnected, rd);
            }
            return responseStreamMsg;
        }

        private static bool IsConnected(ClientWebSocket cws) {
            return cws != null && cws.State == WebSocketState.Open;
        }
        #endregion

        #endregion
        #endregion
    }
}
