using System;
using System.Collections.Generic;

namespace MonkeyPaste {
    public class MpDbSyncEventArgs : EventArgs {
        public object DbObject { get; set; }
        public MpDbLogActionType EventType { get; set; }
        public Dictionary<string, string> UpdatedPropertyLookup { get; set; } = new Dictionary<string, string>();
        public string SourceGuid { get; set; }
    }
}
