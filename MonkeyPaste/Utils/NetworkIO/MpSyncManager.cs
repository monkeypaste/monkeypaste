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

        private MpISyncData _localSyncData;
        #endregion

        #region Properties
        public MpSocketClient SocketClient { get; set; } = new MpSocketClient();

        public MpSocketListener SocketListener { get; set; } = new MpSocketListener();

        public MpCertificateManager CertManager { get; set; }
        
        public MpSessionManager SessionManager { get; set; }
        
        public string StatusLog { get; set; }
        #endregion

        #region Public Methods
        public void Init(MpISyncData localSyncData, string ip, int port) {
            _localSyncData = localSyncData;
            _thisIp = ip;
            _syncPort = port;
            SessionManager = new MpSessionManager(_thisIp, _syncPort);
            SessionManager.AvailableEndPoints.CollectionChanged += EndPoints_CollectionChanged;            
        }

        public void Dispose() {
            SessionManager.Dispose();
            SocketClient.Dispose();
            SocketListener.Dispose();
        }
        #endregion

        #region Private Methods

        #region Event Handlers
        private async void EndPoints_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if(!SessionManager.IsConnected) {
                return;
            }
            if (SessionManager.IsHost) {
                //this device will become the server since its connected and 
                //there are no other clients
                SocketListener.StartListening(_thisIp, _syncPort);
                SocketListener.OnReceive += SocketListener_OnReceive;
            } else if(SessionManager.CanSync) {
                SocketClient.StartClient(SessionManager.HostEndPoint.Ip4Address, SessionManager.HostEndPoint.PortNum);
                SocketClient.OnReceive += SocketClient_OnReceive;

                var cil = await _localSyncData.GetLocalData();
                foreach(var c in cil) {
                    SocketClient.Send(c.ToString());
                }
            }
        }

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
