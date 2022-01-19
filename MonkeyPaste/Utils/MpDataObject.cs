using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpDataObject {
        public Dictionary<string,string> DataFormatLookup { get; set; }

        public MpDataObject() {
            DataFormatLookup = new Dictionary<string, string>();
        }
    }
}
