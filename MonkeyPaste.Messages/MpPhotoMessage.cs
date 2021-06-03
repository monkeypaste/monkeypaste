using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MonkeyPaste.Messages {
    public class MpPhotoMessage : MpMessage {
        public MpPhotoMessage() { }
        public MpPhotoMessage(string username) : base(username) { }

        public string Base64Photo { get; set; }
        public string FileEnding { get; set; }

    }
}
