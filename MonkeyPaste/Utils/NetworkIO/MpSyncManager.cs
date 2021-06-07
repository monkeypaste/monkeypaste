using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace MonkeyPaste {
    public class MpSyncManager {
        #region Singleton
        private static readonly Lazy<MpSyncManager> _Lazy = new Lazy<MpSyncManager>(() => new MpSyncManager());
        public static MpSyncManager Instance { get { return _Lazy.Value; } }
        #endregion

        #region Properties
        public MpSocketClient SocketClient { get; set; }

        public MpSocketListener SocketListener { get; set; }
        #endregion

        #region Public Methods
        public void Init() {
        }
        #endregion
    }
}
