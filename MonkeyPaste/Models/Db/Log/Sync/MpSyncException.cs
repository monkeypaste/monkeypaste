﻿using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;

namespace MonkeyPaste {
    public class MpSyncException : MpInternalExceptionBase {
        public MpSyncMesageType ErrorType { get; set; }
        public MpRemoteDevice RemoteDevice { get; set; }
        public MpSyncException(MpSyncMesageType errorType, MpRemoteDevice rd, Exception bex = null) : base() {
            ErrorType = errorType;
            RemoteDevice = rd;
            if (bex != null) {
                MpConsole.WriteTraceLine(bex);
            }
        }
    }
}
