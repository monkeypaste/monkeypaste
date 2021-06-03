using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MonkeyPaste.Messages {
    public class MpPhotoUrlMessage : MpMessage {
        public MpPhotoUrlMessage() { }
        public MpPhotoUrlMessage(string username) : base(username) { }

        public string Url { get; set; }

    }
}
