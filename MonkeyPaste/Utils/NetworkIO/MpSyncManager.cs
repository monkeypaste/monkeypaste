using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpSyncManager {
        #region Singleton
        private static readonly Lazy<MpSyncManager> _Lazy = new Lazy<MpSyncManager>(() => new MpSyncManager());
        public static MpSyncManager Instance { get { return _Lazy.Value; } }
        #endregion

        #region Properties
        public MpSocketClient SocketClient { get; set; }

        public MpSocketListener SocketListener { get; set; }

        public MpCertificateManager CertManager { get; set; }
        
        public MpSessionManager SessionManager { get; set; }
        #endregion

        #region Public Methods
        public void Init() {
            SessionManager = new MpSessionManager();
            Task.Run(async () => {
                while(!SessionManager.IsConnected) {
                    Thread.Sleep(100);
                }
                // do sync stuff if other devices connected
            });
        }
        #endregion
    }
}
