using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste { 
    public enum MpJsMessageType {
        None = 0,
        AddTemplate,
        UpdateTemplate,
        RemoveTemplate
    }

    public class MpJsMessage {
        public string Header { get; set; }
        public object Message { get; set; }
    }
}
