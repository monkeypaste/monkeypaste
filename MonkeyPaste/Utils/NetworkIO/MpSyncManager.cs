using System;
using System.Collections.Generic;
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
        private MpStreamMesageType _lastMessageType = MpStreamMesageType.None;
        private MpISync _localSyncData;
        private bool _isListener;
        #endregion

        #region Properties
        //public MpSocketClient SocketClient { get; set; } = new MpSocketClient();
        //public MpSocketListener SocketListener { get; set; } = new MpSocketListener();

        //public MpSslTcpClient SslClient { get; set; } = new MpSslTcpClient(null);
        //public MpSslTcpServer SslServer { get; set; } = new MpSslTcpServer(null);

        public MpDeviceEndpoint ThisEndpoint { get; set; }
        public MpDeviceEndpoint ServerEndpoint { get; set; }

        public MpSocketClient SocketClient { get; set; }
        public MpSocketListener SocketListener { get; set; }

        public MpSessionManager SessionManager { get; set; }

        public string StatusLog { get; set; }
        #endregion

        #region Public Methods
        public void Init(MpISync localSyncData, bool isListener) {
            _isListener = isListener;
            _localSyncData = localSyncData;
            ThisEndpoint = new MpDeviceEndpoint(localSyncData,
                44376,
                MpHelpers.Instance.GetNewAccessToken(),
                DateTime.Now);
            ThisEndpoint.IsPublic = false;
            ThisEndpoint.DeviceGuid = localSyncData.GetThisClientGuid();

            Task.Run(async () => {
                if (isListener) {
                    if(SocketListener == null) {
                        SocketListener = new MpSocketListener(ThisEndpoint);
                        SocketListener.OnReceive += SocketListener_OnReceive;
                    }
                    SocketListener.Start2(localSyncData);
                } else {

                    if (ServerEndpoint == null) {
                        ServerEndpoint = new MpDeviceEndpoint() {
                            PublicIp4Address = @"107.242.121.35",
                            PrivateIp4Address = @"10.60.240.173",//@"192.168.43.1",
                            PrivatePortNum = 44376,
                            PublicPortNum = 44376,
                            AccessToken = @"<Listener Access Token>",
                            IsPublic = false
                        };
                    }
                    if (SocketClient == null) {
                        SocketClient = new MpSocketClient(ThisEndpoint, ServerEndpoint);
                        SocketClient.OnReceive += SocketClient_OnReceive;
                    }

                    //var sdata = await localSyncData.GetLocalLog();
                    //var data = localSyncData.ConvertToJson(sdata);
                    _lastMessageType = MpStreamMesageType.HandshakeStart;
                    var startHandshakeMessage = MpStreamMessage.Create(
                        _lastMessageType, 
                        ThisEndpoint.ToString(), localSyncData.GetThisClientGuid());

                    SocketClient.SendMessage(startHandshakeMessage.ToString());
                }
            });
                       
        }

        public void Upload() {

        }

        public void Dispose() {
            SessionManager.Dispose();
        }
        #endregion

        #region Private Methods
        
        #region Event Handlers
        

        private void SocketClient_OnReceive(object sender, string e) {
            MpConsole.WriteLine(@"Message From Listener: " + e);
        }

        private void SocketListener_OnReceive(object sender, string e) {
            Task.Run(async () => {
                var msgHeader = MpStreamMessage.ParseHeader(e);
                MpStreamMessage responeStreamMessage = null;
                switch(msgHeader.MessageType) {
                    case MpStreamMesageType.HandshakeStart:
                        _lastMessageType = MpStreamMesageType.HandshakeBack;

                        responeStreamMessage = MpStreamMessage.Create(
                            _lastMessageType,
                            ThisEndpoint.ToString(), _localSyncData.GetThisClientGuid());

                        break;
                }
                SocketListener.OutMessageQueue.Add(responeStreamMessage.ToString());
                //var dbMsg = await MpDbMessage.Parse(e, new MpStringToDbModelTypeConverter());
                //var streamMessage = await _localSyncData.ProcessRemoteDbLog(dbMsg);
            });            
        }
        #endregion
        #endregion
    }
}
