using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Collections.Generic;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public class MpAvAndroidProcessWatcher : MpAvProcessWatcherBase {
        protected override nint GetParentHandleAtPoint(MpPoint poIntPtr) {
            return App.Current.GetMainWindowHandle();
        }

        public override nint SetActiveProcess(nint handle) {
            return handle;
        }

        protected override nint SetActiveProcess(nint handle, ProcessWindowStyle windowStyle) {
            return handle;
        }

        protected override bool IsAdmin(object handleIdOrTitle) {
            return false;
        }

        protected override ProcessWindowStyle GetWindowStyle(object handleIdOrTitle) {
            return ProcessWindowStyle.Maximized;
        }

        private MpPortableProcessInfo _thisAppInfo;
        //protected override MpPortableProcessInfo GetActiveProcessInfo() {
        //    if (_thisAppInfo == null) {
        //        _thisAppInfo = new MpPortableProcessInfo() {
        //            Handle = App.Current.GetMainWindowHandle(),
        //            ProcessPath = Mp.Services.PlatformInfo.ExecutingPath,
        //            MainWindowTitle = Mp.Services.ThisAppInfo.ThisAppProductName,
        //            MainWindowIconBase64 = MpBase64Images.AppIcon
        //        };
        //    }
        //    return _thisAppInfo;
        //}



        protected override string GetProcessPath(nint handle) {
            return Mp.Services.PlatformInfo.ExecutingPath;
        }

        protected override MpPortableProcessInfo GetProcessInfoByHandle(nint handle) {
            if (_thisAppInfo == null) {
                _thisAppInfo = new MpPortableProcessInfo() {
                    Handle = App.Current.GetMainWindowHandle(),
                    ProcessPath = Mp.Services.PlatformInfo.ExecutingPath,
                    MainWindowTitle = Mp.Services.ThisAppInfo.ThisAppProductName,
                    MainWindowIconBase64 = MpBase64Images.AppIcon
                };
            }
            return _thisAppInfo;
        }

        public override IEnumerable<MpPortableProcessInfo> AllWindowProcessInfos { get; }

        protected override nint GetActiveProcessHandle() {
            return App.Current.GetMainWindowHandle();
        }

        protected override bool IsHandleWindowProcess(nint handle) {
            return true;
        }
    }
}

