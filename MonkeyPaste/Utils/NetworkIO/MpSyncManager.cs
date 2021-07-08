using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MonkeyPaste {
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

        private TcpListener _listener;
        private TcpClient _client;
        #endregion

        public event EventHandler<string> OnError;
        #region Properties
        //public MpSocketClient SocketClient { get; set; } = new MpSocketClient();
        //public MpSocketListener SocketListener { get; set; } = new MpSocketListener();

        //public MpSslTcpClient SslClient { get; set; } = new MpSslTcpClient(null);
        //public MpSslTcpServer SslServer { get; set; } = new MpSslTcpServer(null);

        public static string EofToken { get; set; } = "<EOF>";

        public MpDeviceEndpoint ThisEndpoint { get; set; }
        public MpDeviceEndpoint OtherEndpoint { get; set; }

        //public MpSocketClient SocketClient { get; set; }
        //public MpSocketListener SocketListener { get; set; }

        public MpSessionManager SessionManager { get; set; }

        public string StatusLog { get; set; }

        public MpSyncMesageType LastSyncState { get; set; } = MpSyncMesageType.None;

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

                var sw = new Stopwatch();
                sw.Start();
                MpConsole.WriteLine(@"Attempting Listener discovery...");
                
                //var privateClient = await MpPrivateEndpointDiscoveryHelper.Discover(
                //    ThisEndpoint.PrivateIp4Address,
                //    ThisEndpoint.PrivatePortNum, 
                //    localSync.IsWpf());
                sw.Stop();
                MpConsole.WriteLine(@"Private ip sweep took {0} ms" + sw.ElapsedMilliseconds);

                // Cli

                //if(privateClient == null) {
                if(localSync.IsWpf()) { 
                    //no listener found so start listening
                    MpConsole.WriteLine($"No listener found. Creating listener: {ThisEndpoint}");

                    _listener = new TcpListener(ThisEndpoint.PrivateIPEndPoint);
                    _listener.Start();

                    _client = _listener.AcceptTcpClient();

                    string response = await WaitForResponse();

                    // Receive client endpoint info (clientGuid,AccessToken,etc.)
                    string msg = OpenMessage(response);
                    OtherEndpoint = MpDeviceEndpoint.Parse(msg);

                    // send client  endpoint info
                    Send(ThisEndpoint.ToString());

                    response = await WaitForResponse();

                    // receive client last sync for with listener's deviceGUid
                    msg = OpenMessage(response);
                    var clientLastSyncDateTime = DateTime.Parse(msg);
                    var localSyncDateTime = await _localSyncData.GetLastSyncForRemoteDevice(OtherEndpoint.DeviceGuid);
                    DateTime minLastSyncDateTime = DateTime.MinValue;
                    if(localSyncDateTime > clientLastSyncDateTime) {
                        minLastSyncDateTime = clientLastSyncDateTime;
                    } else {
                        minLastSyncDateTime = localSyncDateTime;
                    }

                    //send dblog for device guid after last sync datetime
                    var localLogFromLastSync = await _localSyncData.GetLocalLog(minLastSyncDateTime);
                    Send(localLogFromLastSync);

                    response = await WaitForResponse();

                    //receive dbo request guid/table name pairs that client needs
                    MpConsole.WriteLine(@"Client db request: " + response);
                } else {
                    var isPublic = false;
                    try {
                        if (isPublic) {

                        } else {
                            _client = new TcpClient(@"192.168.43.209", ThisEndpoint.PrivatePortNum);
                        }
                    }
                    catch (Exception e) {
                        MpConsole.WriteTraceLine("Client connect exction: ", e);
                        OnError?.Invoke(this, e.ToString());
                    }

                    MpConsole.WriteLine($"Connected to a private listener");

                    // send listener endpoint info (clientGuid,accessToken)
                    ThisEndpoint.LoginDateTime = DateTime.Now;
                    string epStr = ThisEndpoint.ToString();
                    MpConsole.WriteLine(@"Sending client endpoint info to listener: " + epStr);
                    Send(epStr);

                    string response = await WaitForResponse();

                    // receive listener endpoint info
                    string msg = OpenMessage(response);
                    OtherEndpoint = MpDeviceEndpoint.Parse(msg);

                    //Send last sync datetime
                    var lastSyncDateTime = await _localSyncData.GetLastSyncForRemoteDevice(OtherEndpoint.DeviceGuid);
                    Send(lastSyncDateTime.ToString());

                    response = await WaitForResponse();

                    //receive db log from listener
                    msg = OpenMessage(response);
                    var neededItemList = await _localSyncData.ProcessRemoteDbLog(msg);

                    //send db obj request to listener
                    Send(neededItemList.ToString());

                    response = await WaitForResponse();

                    //receive db objects from listener
                    msg = OpenMessage(response);
                    MpConsole.WriteLine(@"Objects from listener: " + msg);
                }
            });                       
        }

        public void Dispose() {
            SessionManager.Dispose();
        }
        #endregion

        #region Private Methods
        public async Task<string> WaitForResponse() {
            if (_client == null || !_client.Connected) {
                return null;
            }
            while(true) {
                if(_client.Available > 0) {
                    using (var streamReader = new StreamReader(_client.GetStream())) {
                        string response = streamReader.ReadToEnd();
                        MpConsole.WriteLine(@"Received: " + response);
                        return response;
                    }
                        
                }
                await Task.Delay(100);
            }
        }

        public void Send(string msg) {
            try {
                msg += EofToken;
                //var streamWriter = new StreamWriter(_client.GetStream());
               // streamWriter.Write(msg);
                var msgBytes = Encoding.ASCII.GetBytes(msg);
                _client.GetStream().Write(msgBytes, 0, msgBytes.Length);
                _lastSend = msg;
                MpConsole.WriteLine("Sent: {0}" + msg);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine(@"Socket write exception: ", ex);
                OnError?.Invoke(this, ex.ToString());
            }
        }
        #region Event Handlers

        #region Listener Events

        //private void SocketListener_OnReceive(object sender, string e) {
        //    //_lastMsg = OpenMessage(msg);
        //    //_localSyncData.RunOnMainThread((Action)(() => UpdateListenerSyncState()));
            
        //    string msg = OpenMessage(e);
        //    Task.Run(async () => {                
        //        _lastMsg = string.Empty;
        //        switch (LastSyncState) {
        //            case MpSyncMesageType.None:
        //                OtherEndpoint = MpDeviceEndpoint.Parse(msg);
        //                if (OtherEndpoint == null) {
        //                    // TODO handle error
        //                    return;
        //                }
        //                var lastSyncDateTime = await _localSyncData.GetLastSyncForRemoteDevice(OtherEndpoint.DeviceGuid);
        //                LastSyncState = MpSyncMesageType.HandshakeBack;
        //                SocketListener.Send(lastSyncDateTime.ToString());
        //                break;
        //            case MpSyncMesageType.HandshakeBack: //step 4
        //                var confirmedLastSyncDateTime = DateTime.Parse(msg);
        //                var localLogFromLastSync = await _localSyncData.GetLocalLog(confirmedLastSyncDateTime);
        //                LastSyncState = MpSyncMesageType.ResponseData;
        //                SocketListener.Send(localLogFromLastSync);
        //                break;
        //            case MpSyncMesageType.ResponseData:

        //                break;
        //        }
        //        //var msg = MpStreamMessage.Parse(e);
        //        //MpStreamMessage responeStreamMessage = null;
        //        //switch (msg.Header.MessageType) {
        //        //    case MpStreamMesageType.HandshakeStart:
        //        //        var clientHandshakeMessage = MpHandshakeMessageContent.Parse(e);
        //        //        SyncState = MpStreamMesageType.HandshakeBack;

        //        //        responeStreamMessage = MpStreamMessage.Create(
        //        //            SyncState,
        //        //            ThisEndpoint.ToString(), _localSyncData.GetThisClientGuid());

        //        //        break;
        //        //}
        //        //SocketListener.OutMessageQueue.Add(responeStreamMessage.ToString());


        //        //var dbMsg = await MpDbMessage.Parse(e, new MpStringToDbModelTypeConverter());
        //        //var streamMessage = await _localSyncData.ProcessRemoteDbLog(dbMsg);
        //    });
        //}

        private void SocketListener_OnError(object sender, string e) {
            MpConsole.WriteTraceLine("Listener error: " + e);
        }
        #endregion

        #region Client Events
        //private void SocketClient_OnConnect(object sender, string e) {
        //    Task.Run(async () => {
        //        // send listener endpoint info (clientGuid,accessToken)
        //        LastSyncState = MpSyncMesageType.Connect;
        //        ThisEndpoint.LoginDateTime = DateTime.Now;
        //        string epStr = ThisEndpoint.ToString();
        //        MpConsole.WriteLine(@"Sending client endpoint info to listener: " + epStr);
        //        SocketClient.Send(epStr);

        //        string response = await SocketClient.WaitForResponse();

        //        // receive listener endpoint info
        //        string msg = OpenMessage(response);
        //        OtherEndpoint = MpDeviceEndpoint.Parse(msg);
        //        var lastSyncDateTime = await _localSyncData.GetLastSyncForRemoteDevice(OtherEndpoint.DeviceGuid);
        //        LastSyncState = MpSyncMesageType.HandshakeStart;
        //        SocketClient.Send(lastSyncDateTime.ToString());

        //        response = await SocketClient.WaitForResponse();

        //        // send last sync datetime
        //        msg = OpenMessage(response);
        //        var listenerLastSyncDateTime = DateTime.Parse(msg);
        //        var localLastSyncDateTime = await _localSyncData.GetLastSyncForRemoteDevice(OtherEndpoint.DeviceGuid);
        //        var earliestLastSyncDateTime = listenerLastSyncDateTime.Ticks > localLastSyncDateTime.Ticks ? localLastSyncDateTime : listenerLastSyncDateTime;
        //        LastSyncState = MpSyncMesageType.RequestLog;
        //        SocketClient.Send(earliestLastSyncDateTime.ToString());

        //        response = await SocketClient.WaitForResponse();

        //        // receive min last sync datetime
        //        msg = OpenMessage(response);
        //        var neededItemList = await _localSyncData.ProcessRemoteDbLog(msg);
        //        LastSyncState = MpSyncMesageType.RequestData;
        //        SocketClient.Send(neededItemList.ToString());
        //    });
        //}

        //private void SocketClient_OnConnectOrReceive(object sender, string e) {
        //    string msg = OpenMessage(e);
        //    Task.Run(async () => {
        //        //var msgHeader = MpStreamMessage.Parse(e);
        //        //MpStreamMessage responeStreamMessage = null;
        //        switch (LastSyncState) {
                    
        //            case MpSyncMesageType.Connect:
        //                OtherEndpoint = MpDeviceEndpoint.Parse(msg);
        //                var lastSyncDateTime = await _localSyncData.GetLastSyncForRemoteDevice(OtherEndpoint.DeviceGuid);
        //                LastSyncState = MpSyncMesageType.HandshakeStart;
        //                SocketClient.Send(lastSyncDateTime.ToString());
        //                break;
        //            case MpSyncMesageType.HandshakeStart: //step 3
        //                var listenerLastSyncDateTime = DateTime.Parse(msg);
        //                var localLastSyncDateTime = await _localSyncData.GetLastSyncForRemoteDevice(OtherEndpoint.DeviceGuid);
        //                var earliestLastSyncDateTime = listenerLastSyncDateTime.Ticks > localLastSyncDateTime.Ticks ? localLastSyncDateTime : listenerLastSyncDateTime;
        //                LastSyncState = MpSyncMesageType.RequestLog;
        //                SocketClient.Send(earliestLastSyncDateTime.ToString());
        //                break;
        //            case MpSyncMesageType.RequestLog:
        //                var neededItemList = await _localSyncData.ProcessRemoteDbLog(msg);
        //                LastSyncState = MpSyncMesageType.RequestData;
        //                SocketClient.Send(neededItemList.ToString());
        //                break;
        //        }
        //        //SocketListener.OutMessageQueue.Add(responeStreamMessage.ToString());

        //    });
        //}

        private void SocketClient_OnError(object sender, string e) {
            MpConsole.WriteTraceLine("Client error: " + e);
        }
        #endregion

        private string OpenMessage(string e) {
            if (!ValidateMessagee(e)) {
                throw new Exception("Message either has a bad check sum or no <Eof>");
            } 
            return e.Replace(MpSocket.EofToken, string.Empty);
        }

        private bool ValidateMessagee(string msg) {
            if(msg == null) {
                return false;
            }
            return msg.Contains(MpSocket.EofToken);
        }
        #endregion

        #endregion
    }
}
