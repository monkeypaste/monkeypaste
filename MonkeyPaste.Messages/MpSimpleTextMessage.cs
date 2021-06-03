using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MonkeyPaste.Messages {
    public class MpSimpleTextMessage : MpMessage {
        public MpSimpleTextMessage() { }
        public MpSimpleTextMessage(string username) : base(username) { }
        public string Text { get; set; }
}

}
