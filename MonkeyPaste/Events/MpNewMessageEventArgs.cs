using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MonkeyPaste.Messages;

namespace MonkeyPaste {
    public class MpNewMessageEventArgs : EventArgs {
        public MpMessage Message { get; private set; }
        public MpNewMessageEventArgs(MpMessage message) {
            Message = message;
}
}

}
