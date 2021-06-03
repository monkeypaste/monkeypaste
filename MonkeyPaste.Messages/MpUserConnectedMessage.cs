using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MonkeyPaste.Messages {
    public class MpUserConnectedMessage : MpMessage {
        public MpUserConnectedMessage() { }
        public MpUserConnectedMessage(string username) : base(username) { }
}

}
