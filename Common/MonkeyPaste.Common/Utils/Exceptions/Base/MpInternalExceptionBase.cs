using System;

namespace MonkeyPaste.Common {
    public abstract class MpInternalExceptionBase : Exception {
        public MpInternalExceptionBase() : base() { }
        public MpInternalExceptionBase(string msg) : base(msg) { }
        public MpInternalExceptionBase(string msg, Exception innerException) : base(msg, innerException) { }
    }
}
