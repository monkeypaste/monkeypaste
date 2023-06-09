using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MonkeyPaste.Common {
    public class MpPortableProcessInfo {
        public IntPtr Handle { get; set; } = IntPtr.Zero;
        public string ProcessPath { get; set; } = string.Empty;
        public string ProcessName { get; set; } // app name

        public string MainWindowTitle { get; set; }

        public string MainWindowIconBase64 { get; set; }

        public DateTime LastActiveDateTime { get; set; }

        public List<string> ArgumentList { get; set; } = new List<string>();
        public string Arguments { get; set; }

        public ProcessWindowStyle WindowState { get; set; }

        public override string ToString() {
            return string.Format(@"Title '{0}' Handle '{1}' Path '{2}' ", MainWindowTitle, Handle, ProcessPath);
        }
    }

}
