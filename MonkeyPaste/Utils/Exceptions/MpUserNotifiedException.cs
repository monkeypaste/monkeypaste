using System;

namespace MonkeyPaste {
    public class MpUserNotifiedException : MpException {
        public MpUserNotifiedException() : base() { }
        public MpUserNotifiedException(string msg) : base(msg) { }
        public MpUserNotifiedException(string msg, Exception innerException) : base(msg, innerException) { }
    }
}
