using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {

    public partial class MpAvProcessWatcher {
        private MpPortableProcessInfo __thisAppInfo;
        MpPortableProcessInfo _thisAppInfo {
            get {
                if (__thisAppInfo == null) {
                    __thisAppInfo = new MpPortableProcessInfo() {
                        Handle = App.Current.GetMainWindowHandle(),
                        ProcessPath = Mp.Services.PlatformInfo.ExecutingPath,
                        MainWindowTitle = Mp.Services.ThisAppInfo.ThisAppProductName,
                        MainWindowIconBase64 = MpBase64Images.AppIcon
                    };
                }
                return __thisAppInfo;
            }
        }

        public nint SetActiveProcess(MpPortableProcessInfo p) {
            return GetThisAppHandle();
        }

        public MpPortableProcessInfo GetClipboardOwner() {
            return _thisAppInfo;
        }
        protected IntPtr GetParentHandleAtPoint(MpPoint p) {
            return GetThisAppHandle();
        }
        protected IntPtr GetThisAppHandle() {
            return App.Current.GetMainWindowHandle();
        }
        protected IntPtr GetActiveProcessHandle() {
            return GetThisAppHandle();
        }
        protected string GetProcessPath(IntPtr handle) {
            return Mp.Services?.PlatformInfo?.ExecutingPath;
        }
        protected string GetAppNameByProessPath(string path) {
            return Mp.Services?.ThisAppInfo?.ThisAppProductName;
        }
        protected string GetProcessTitle(IntPtr handle) {
            return Mp.Services?.ThisAppInfo?.ThisAppProductName;
        }
        protected bool IsHandleWindowProcess(IntPtr handle) {
            return true;
        }
        public IEnumerable<MpPortableProcessInfo> AllWindowProcessInfos { get; } = [];
    }
}
