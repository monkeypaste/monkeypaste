using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpProcessActivatedEventArgs : EventArgs {
        public string ProcessPath { get; set; }
        public string ApplicationName { get; set; }
        public IntPtr Handle { get; set; }
    }
}
