using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpUserNotifiedException : MpException {
        public MpUserNotifiedException() : base() { }
        public MpUserNotifiedException(string msg) : base(msg) { }
        public MpUserNotifiedException(string msg, Exception innerException) : base(msg, innerException) { }
    }
}
