using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Common {
    public class MpPortableProcessInfo {
        public IntPtr Handle { get; set; } = IntPtr.Zero;
        public string ProcessPath { get; set; } = string.Empty;

        public string MainWindowTitle { get; set; }

        public string MainWindowIconBase64 { get; set; }

        public DateTime LastActiveDateTime { get; set; }

        public List<string> ArgumentList { get; set; }       

        public string WindowState { get; set; }

        public override string ToString() {
            return string.Format(@"Title '{0}' Handle '{1}' Path '{2}' ", MainWindowTitle, Handle, ProcessPath);
        }
    }
}
