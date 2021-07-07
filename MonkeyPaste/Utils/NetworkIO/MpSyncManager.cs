using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
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
        #endregion

        #region Properties
        //public MpSocketClient SocketClient { get; set; } = new MpSocketClient();
        //public MpSocketListener SocketListener { get; set; } = new MpSocketListener();

        //public MpSslTcpClient SslClient { get; set; } = new MpSslTcpClient(null);
        //public MpSslTcpServer SslServer { get; set; } = new MpSslTcpServer(null);

        public MpDeviceEndpoint ThisEndpoint { get; set; }
        public MpDeviceEndpoint OtherEndpoint { get; set; }

        public MpSocketClient SocketClient { get; set; }
        public MpSocketListener SocketListener { get; set; }

        public MpSessionManager SessionManager { get; set; }

        public string StatusLog { get; set; }

        public MpSyncMesageType LastSyncState { get; set; } = MpSyncMesageType.None;

        public bool IsClient => SocketClient != null;
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
                string listenerIp = await MpPrivateEndpointDiscoveryHelper.Discover(
                    ThisEndpoint.PrivateIp4Address,
                    ThisEndpoint.PrivatePortNum, 
                    localSync.IsWpf());
                sw.Stop();
                MpConsole.WriteLine(@"Private ip sweep took {0} ms" + sw.ElapsedMilliseconds);

                if(string.IsNullOrEmpty(listenerIp)) {
                    //no listener found so start listening
                    MpConsole.WriteLine($"No listener found. Creating listener: {ThisEndpoint}");

                    SocketListener = new MpSocketListener();
                    SocketListener.OnReceive += SocketListener_OnReceive;
                    SocketListener.OnListenerError += SocketListener_OnError;

                    SocketListener.StartListening(ThisEndpoint);
                    
                    // TODO Add Public Discovery here 
                } else {

                    MpConsole.WriteLine($"Connected to listener: {listenerIp}");

                    SocketClient = new MpSocketClient();
                    SocketClient.OnConnect += SocketClient_OnConnect;
                    SocketClient.OnReceive += SocketClient_OnReceive;
                    SocketClient.OnClientError += SocketClient_OnError;

                    SocketClient.Connect(listenerIp,ThisEndpoint.PrivatePortNum, false);
                }
            });                       
        }

        public void Dispose() {
            SessionManager.Dispose();
        }
        #endregion

        #region Private Methods
        #region Event Handlers
        
        #region Listener Events

        private void SocketListener_OnReceive(object sender, string e) {
            //_lastMsg = OpenMessage(msg);
            //_localSyncData.RunOnMainThread((Action)(() => UpdateListenerSyncState()));
            
            string msg = OpenMessage(e);
            Task.Run(async () => {                
                _lastMsg = string.Empty;
                switch (LastSyncState) {
                    case MpSyncMesageType.None:
                        OtherEndpoint = MpDeviceEndpoint.Parse(msg);
                        if (OtherEndpoint == null) {
                            // TODO handle error
                            return;
                        }
                        var lastSyncDateTime = await _localSyncData.GetLastSyncForRemoteDevice(OtherEndpoint.DeviceGuid);
                        LastSyncState = MpSyncMesageType.HandshakeBack;
                        SocketListener.Send(lastSyncDateTime.ToString());
                        break;
                    case MpSyncMesageType.HandshakeBack: //step 4
                        var confirmedLastSyncDateTime = DateTime.Parse(msg);
                        var localLogFromLastSync = await _localSyncData.GetLocalLog(confirmedLastSyncDateTime);
                        LastSyncState = MpSyncMesageType.ResponseData;
                        SocketListener.Send(localLogFromLastSync);
                        break;
                    case MpSyncMesageType.ResponseData:

                        break;
                }
                //var msg = MpStreamMessage.Parse(e);
                //MpStreamMessage responeStreamMessage = null;
                //switch (msg.Header.MessageType) {
                //    case MpStreamMesageType.HandshakeStart:
                //        var clientHandshakeMessage = MpHandshakeMessageContent.Parse(e);
                //        SyncState = MpStreamMesageType.HandshakeBack;

                //        responeStreamMessage = MpStreamMessage.Create(
                //            SyncState,
                //            ThisEndpoint.ToString(), _localSyncData.GetThisClientGuid());

                //        break;
                //}
                //SocketListener.OutMessageQueue.Add(responeStreamMessage.ToString());


                //var dbMsg = await MpDbMessage.Parse(e, new MpStringToDbModelTypeConverter());
                //var streamMessage = await _localSyncData.ProcessRemoteDbLog(dbMsg);
            });
        }

        private void SocketListener_OnError(object sender, string e) {
            MpConsole.WriteTraceLine("Listener error: " + e);
        }
        #endregion

        #region Client Events
        private void SocketClient_OnConnect(object sender, TcpClient client) {
            LastSyncState = MpSyncMesageType.Connect;
            ThisEndpoint.LoginDateTime = DateTime.Now;
            string epStr = ThisEndpoint.ToString();
            MpConsole.WriteLine(@"Sending client endpoint info to listener: " + epStr);
            SocketClient.Send(epStr);
        }

        private void SocketClient_OnReceive(object sender, string e) {
            string msg = OpenMessage(e);
            Task.Run(async () => {
                //var msgHeader = MpStreamMessage.Parse(e);
                //MpStreamMessage responeStreamMessage = null;
                switch (LastSyncState) {
                    case MpSyncMesageType.Connect:
                        OtherEndpoint = MpDeviceEndpoint.Parse(msg);
                        var lastSyncDateTime = await _localSyncData.GetLastSyncForRemoteDevice(OtherEndpoint.DeviceGuid);
                        LastSyncState = MpSyncMesageType.HandshakeStart;
                        SocketClient.Send(lastSyncDateTime.ToString());
                        break;
                    case MpSyncMesageType.HandshakeStart: //step 3
                        var listenerLastSyncDateTime = DateTime.Parse(msg);
                        var localLastSyncDateTime = await _localSyncData.GetLastSyncForRemoteDevice(OtherEndpoint.DeviceGuid);
                        var earliestLastSyncDateTime = listenerLastSyncDateTime.Ticks > localLastSyncDateTime.Ticks ? localLastSyncDateTime : listenerLastSyncDateTime;
                        LastSyncState = MpSyncMesageType.RequestLog;
                        SocketClient.Send(earliestLastSyncDateTime.ToString());
                        break;
                    case MpSyncMesageType.RequestLog:
                        var neededItemList = await _localSyncData.ProcessRemoteDbLog(msg);
                        LastSyncState = MpSyncMesageType.RequestData;
                        SocketClient.Send(neededItemList.ToString());
                        break;
                }
                //SocketListener.OutMessageQueue.Add(responeStreamMessage.ToString());

            });
        }
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
