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
        public void Init(MpISync localSync, bool isListener) {     
            Task.Run(async () => {
                while(!MpHelpers.Instance.IsConnectedToInternet()) {
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
                    SocketListener.OnReceive += SocketListener_OnReceive;
                    SocketListener.StartListening();
                } else {
                    MpConsole.WriteLine($"Created client: {ThisEndpoint}");
                    SocketClient.OnReceive += SocketClient_OnReceive;
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
        
        #region Event Handlers
        

        private void SocketClient_OnReceive(object sender, string e) {
            MpConsole.WriteLine(@"Message From Listener: " + e);
        }

        private void SocketListener_OnReceive(object sender, string e) {
            return;
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
