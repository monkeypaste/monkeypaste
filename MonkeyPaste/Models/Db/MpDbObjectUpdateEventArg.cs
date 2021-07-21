using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpDbObjectUpdateEventArg : EventArgs {
        public MpDbModelBase DbObject { get; set; }
        public Dictionary<string, string> UpdatedPropertyLookup { get; set; } = new Dictionary<string, string>();        
    }
}
