using System;

namespace MonkeyPaste {
    public class MpException : Exception {
        public MpException() : base() { }
        public MpException(string msg) : base(msg) { }
        public MpException(string msg, Exception innerException) : base(msg, innerException) { }
    }

}
