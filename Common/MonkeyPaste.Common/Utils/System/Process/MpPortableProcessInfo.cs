using System;
using System.Collections.Generic;

namespace MonkeyPaste.Common {
    public class MpPortableProcessInfo {
        public IntPtr Handle { get; set; } = IntPtr.Zero;
        public string ProcessPath { get; set; } = string.Empty;
        public string ApplicationName { get; set; } // app name

        public string MainWindowTitle { get; set; }
        public string MainWindowIconBase64 { get; set; }

        public List<string> ArgumentList { get; set; } = new List<string>();

        public static MpPortableProcessInfo Create(string path) {
            if (path.IsFile()) {
                return new MpPortableProcessInfo() { ProcessPath = path };
            }
            return null;
        }
        public MpPortableProcessInfo() { }
        public bool IsThisAppProcess() {
            return MpCommonTools.Services.ProcessWatcher.IsProcessPathEqual(Handle, MpCommonTools.Services.ProcessWatcher.ThisAppProcessInfo.Handle);
        }
        public bool IsHandleProcess(IntPtr handle) {
            return MpCommonTools.Services.ProcessWatcher.IsProcessPathEqual(Handle, handle);
        }

        public override string ToString() {
            //return string.Format(@"Handle '{0}' Path '{1}' Title '{2}' ", Handle, ProcessPath, MainWindowTitle);
            return MpJsonConverter.SerializeObject(this);
        }

        public object Clone() {
            return new MpPortableProcessInfo() {
                Handle = Handle,
                ProcessPath = ProcessPath,
                MainWindowTitle = MainWindowTitle,
                ApplicationName = ApplicationName,
                ArgumentList = ArgumentList
            };
        }
    }

}
