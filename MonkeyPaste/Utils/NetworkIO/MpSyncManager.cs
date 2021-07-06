using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        public void Init(MpISync localSync, bool isListener) {     
            Task.Run(async () => {
                while(!MpHelpers.Instance.IsConnectedToNetwork()) {
                    Thread.Sleep(10000);
                }
                _isListener = isListener;
                _localSyncData = localSync;
                ThisEndpoint = new MpDeviceEndpoint(localSync,
                    44376,
                    MpHelpers.Instance.GetNewAccessToken(),
                    DateTime.Now);
                ThisEndpoint.IsPublic = false;

                var sw = new Stopwatch();
                sw.Start();
                SocketClient = await MpSocketClient.TryPrivateCreateAndConnect(ThisEndpoint,localSync.IsWpf());
                sw.Stop();
                MpConsole.WriteLine(@"Private ip sweep took {0} ms" + sw.ElapsedMilliseconds);

                if(SocketClient == null) {
                    MpConsole.WriteLine($"No listener found. Creating listener: {ThisEndpoint}");
                    SocketListener = new MpSocketListener(ThisEndpoint);
                    SocketListener.OnConnect += SocketListener_OnConnect;
                    SocketListener.OnSend += SocketListener_OnSend;
                    SocketListener.OnReceive += SocketListener_OnReceive;
                    SocketListener.OnError += SocketListener_OnError;

                    SocketListener.StartListening();
                } else {
                    MpConsole.WriteLine($"Created client: {ThisEndpoint}");
                    SocketClient.OnConnect += SocketClient_OnConnect;
                    SocketClient.OnSend += SocketClient_OnSend;
                    SocketClient.OnReceive += SocketClient_OnReceive;
                    SocketClient.OnError += SocketClient_OnError;

                    SocketClient.StartClient();
                }
                //    //var sdata = await localSyncData.GetLocalLog();
                //    //var data = localSyncData.ConvertToJson(sdata);

                //    //_lastMessageType = MpStreamMesageType.HandshakeStart;
                //    //var startHandshakeMessage = MpStreamMessage.Create(
                //    //    _lastMessageType, 
                //    //    ThisEndpoint.ToString(), localSyncData.GetThisClientGuid());

                //    //SocketClient.SendMessage(startHandshakeMessage.ToString());
                //}
            });
                       
        }

       

        public void Dispose() {
            SessionManager.Dispose();
        }
        #endregion

        #region Private Methods
        private void UpdateSyncState(string msg) {
           
        }
        #region Event Handlers
        
        #region Listener Events
        
        private void SocketListener_OnConnect(object sender, string e) {

        }

        private void SocketListener_OnSend(object sender, string e) {
            //throw new NotImplementedException();
        }

        private void SocketListener_OnReceive(object sender, string e) {
            string msg = OpenMessage(e);
            Task.Run(async () => {
                switch(LastSyncState) {
                    case MpSyncMesageType.None:
                        OtherEndpoint = JsonConvert.DeserializeObject<MpDeviceEndpoint>(msg);
                        var lastSyncDateTime = await _localSyncData.GetLastSyncForRemoteDevice(OtherEndpoint.DeviceGuid);
                        LastSyncState = MpSyncMesageType.HandshakeBack;
                        SocketListener.Send(lastSyncDateTime.ToString());
                        break;
                    case MpSyncMesageType.HandshakeBack: //step 4
                        var confirmedLastSyncDateTime = DateTime.Parse(msg);
                        var localLogFromLastSync = await _localSyncData.GetLocalLog(confirmedLastSyncDateTime);
                        LastSyncState = MpSyncMesageType.ResponseData;
                        SocketClient.Send(localLogFromLastSync);
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
            throw new NotImplementedException();
        }
        #endregion

        #region Client Events
        private void SocketClient_OnConnect(object sender, string e) {
            Task.Run(async () => {
                await Task.Delay(1);
                LastSyncState = MpSyncMesageType.Connect;
                var thisEndpointJson = JsonConvert.SerializeObject(ThisEndpoint);
                SocketClient.Send(thisEndpointJson);
            });
        }

        private void SocketClient_OnSend(object sender, string e) {
            //throw new NotImplementedException();
        }
        private void SocketClient_OnReceive(object sender, string e) {
            string msg = OpenMessage(e);
            Task.Run(async () => {
                //var msgHeader = MpStreamMessage.Parse(e);
                //MpStreamMessage responeStreamMessage = null;
                switch (LastSyncState) {
                    case MpSyncMesageType.Connect:
                        OtherEndpoint = JsonConvert.DeserializeObject<MpDeviceEndpoint>(msg);
                        var lastSyncDateTime = await _localSyncData.GetLastSyncForRemoteDevice(OtherEndpoint.DeviceGuid);                        
                        LastSyncState = MpSyncMesageType.HandshakeStart;
                        SocketClient.Send(lastSyncDateTime.ToString());
                        break;
                    case MpSyncMesageType.HandshakeStart: //step 3
                        var listenerLastSyncDateTime = DateTime.Parse(msg);
                        var localLastSyncDateTime = await _localSyncData.GetLastSyncForRemoteDevice(OtherEndpoint.DeviceGuid);
                        var earliestLastSyncDateTime = listenerLastSyncDateTime.Ticks > localLastSyncDateTime.Ticks ? localLastSyncDateTime:listenerLastSyncDateTime;
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
            throw new NotImplementedException();
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
