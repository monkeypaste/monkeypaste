using MonkeyPaste.Common;
using System;

namespace MonkeyPaste.Common {
    public class MpUserNotifiedException : MpInternalExceptionBase {
        public MpUserNotifiedException() : base() { }
        public MpUserNotifiedException(string msg) : base(msg) { }
        public MpUserNotifiedException(string msg, Exception innerException) : base(msg, innerException) { }
    }
}
