using System;
using MonkeyPaste.Plugin;

namespace MonkeyPaste {
    public class MpSyncException : Exception {
        public MpSyncMesageType ErrorType { get; set; }
        public MpRemoteDevice RemoteDevice { get; set; }
        public MpSyncException(MpSyncMesageType errorType, MpRemoteDevice rd, Exception bex = null) : base() {
            ErrorType = errorType;
            RemoteDevice = rd;
            if(bex != null) {
                MpConsole.WriteTraceLine(bex);
            }
        }
    }
}
