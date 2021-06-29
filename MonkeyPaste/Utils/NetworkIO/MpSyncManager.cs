using System;
using System.Collections.Generic;
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
        private string _thisIp = string.Empty;
        private int _syncPort = -1;
        private string _at = string.Empty;
        private MpISyncData _localSyncData;
        #endregion

        #region Properties
        //public MpSocketClient SocketClient { get; set; } = new MpSocketClient();
        //public MpSocketListener SocketListener { get; set; } = new MpSocketListener();

        //public MpSslTcpClient SslClient { get; set; } = new MpSslTcpClient(null);
        //public MpSslTcpServer SslServer { get; set; } = new MpSslTcpServer(null);

        
        
        public MpSessionManager SessionManager { get; set; }
        
        public string StatusLog { get; set; }
        #endregion

        #region Public Methods
        public void Init(MpISyncData localSyncData, string ip) {
            _localSyncData = localSyncData;
            _thisIp = ip;

            var client = MpSocketClient.CreateClient(
                new MpDeviceEndpoint(@"206.72.205.68", 44376, @"Access Token", DateTime.Now),
                (s, e) => {
                    Console.WriteLine(@"Sent: " + e);
                },
                (s, e) => {
                    Console.WriteLine(@"Received: " + e);
                },
                (s, e) => {
                    Console.WriteLine(@"Error: " + e);
                }
            );
            client.Send(@"Hi Server!");
            //_syncPort = 443;

            //SocketListener.OnReceive += SocketListener_OnReceive;
            //SocketClient.OnReceive += SocketClient_OnReceive;

            //SessionManager = new MpSessionManager(_thisIp, _syncPort);

            //SessionManager.OnConnected += async (s,e) => {
            //    var data = _localSyncData.GetLocalData();
            //    await SessionManager.Upload(data);
            //};
            //SessionManager.AvailableEndPoints.CollectionChanged += EndPoints_CollectionChanged;            
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
            throw new NotImplementedException();
        }

        private void SocketListener_OnReceive(object sender, string e) {
            throw new NotImplementedException();
        }
        #endregion
        #endregion
    }
}
