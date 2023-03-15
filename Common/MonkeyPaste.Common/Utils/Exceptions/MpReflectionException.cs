using System;

namespace MonkeyPaste.Common {
    public class MpReflectionException : MpInternalExceptionBase {
        public MpReflectionException() : base() { }
        public MpReflectionException(string msg) : base(msg) { }
        public MpReflectionException(string msg, Exception innerException) : base(msg, innerException) { }
    }
}
