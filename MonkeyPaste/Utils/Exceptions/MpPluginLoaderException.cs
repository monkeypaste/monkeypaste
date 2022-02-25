using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpPluginLoaderException : Exception {
        public MpPluginLoaderException(string msg) : base(msg) { }
        public MpPluginLoaderException(string msg, Exception innerException) : base(msg, innerException) { }
    }
}
