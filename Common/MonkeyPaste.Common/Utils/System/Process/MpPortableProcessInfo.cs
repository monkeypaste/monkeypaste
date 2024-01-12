using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Common {
    public class MpPortableProcessInfo : MpIIsValueEqual<MpPortableProcessInfo> {
        #region Interfaces

        #region MpIIsValueEqual Implementation

        public bool IsValueEqual(MpPortableProcessInfo other) {
            return
                ProcessPath.ToLower() == other.ProcessPath.ToLower() &&
                !ArgumentList.Difference(other.ArgumentList).Any();
        }

        #endregion

        #endregion
        #region Properties
        public nint Handle { get; set; }// = nint.Zero;
        public int WindowNumber { get; set; }
        public string ProcessPath { get; set; } = string.Empty;
        public string ApplicationName { get; set; } // app name

        public string MainWindowTitle { get; set; }
        public string MainWindowIconBase64 { get; set; }

        public List<string> ArgumentList { get; set; } = new List<string>();

        #endregion
        #region Statics
        public static MpPortableProcessInfo FromPath(string path) {
            if (path.IsFile()) {
                return new MpPortableProcessInfo() { ProcessPath = path };
            }
            return null;
        }
        public static MpPortableProcessInfo FromHandle(nint handle) {
            if (handle != 0) {
                return new MpPortableProcessInfo() { Handle = handle };
            }
            return null;
        }
        #endregion

        #region Constructors

        public MpPortableProcessInfo() { }
        public MpPortableProcessInfo(string path) { ProcessPath = path; }
        #endregion

        #region Public Methods
        public bool IsThisAppProcess() {
            return MpCommonTools.Services.ProcessWatcher.IsProcessPathEqual(this, MpCommonTools.Services.ProcessWatcher.ThisAppProcessInfo);
        }

        public override string ToString() {
            //return string.Format(@"Handle '{0}' Path '{1}' Title '{2}' ", Handle, ProcessPath, MainWindowTitle);
            return this.SerializeObject();
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


        #endregion
    }

}
