using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace MonkeyPaste.Common {
    public class MpPortableProcessInfo : MpIIsValueEqual<MpPortableProcessInfo> {
        #region Interfaces

        #region MpIIsValueEqual Implementation

        public bool IsValueEqual(MpPortableProcessInfo other) {
            return
                ProcessPath.ToLowerInvariant() == other.ProcessPath.ToLowerInvariant() &&
                !ArgumentList.Difference(other.ArgumentList).Any();
        }

        #endregion

        #endregion
        #region Properties
        public nint Handle { get; set; }// = 0;
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
                return new MpPortableProcessInfo() {
                    ProcessPath = path
                };
            }
            return null;
        }
        public static MpPortableProcessInfo FromHandle(nint handle, bool fallback_on_error) {
            if (MpCommonTools.Services != null &&
                MpCommonTools.Services.ProcessWatcher != null) {
                var pi = MpCommonTools.Services.ProcessWatcher.GetProcessInfoFromHandle(handle);
                if(fallback_on_error && (pi == null || pi.ProcessPath.IsNullOrEmpty())) {
                    // fallback to os thing
                    return FromPath(MpCommonTools.Services.PlatformInfo.OsFileManagerPath);
                }
                return pi;
            }
            return null;
        }
        #endregion

        #region Constructors

        public MpPortableProcessInfo() { }
        #endregion

        #region Public Methods
        public bool IsThisAppProcess() {
            return MpCommonTools.Services.ProcessWatcher.IsProcessPathEqual(this, MpCommonTools.Services.ProcessWatcher.ThisAppProcessInfo);
        }

        public override string ToString() {
            return $"Handle '{Handle}' Title '{MainWindowTitle}' Path '{ProcessPath}' Args '{string.Join(" ",ArgumentList)}'";
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
