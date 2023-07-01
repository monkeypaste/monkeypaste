using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public class MpAvAndroidProcessWatcher : MpAvProcessWatcherBase {
        protected override nint GetParentHandleAtPoint(MpPoint poIntPtr) {
            return App.Current.GetMainWindowHandle();
        }

        protected override nint SetActiveProcess(nint handle) {
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
        protected override MpPortableProcessInfo GetActiveProcessInfo() {
            if (_thisAppInfo == null) {
                _thisAppInfo = new MpPortableProcessInfo() {
                    Handle = App.Current.GetMainWindowHandle(),
                    ProcessPath = Mp.Services.PlatformInfo.ExecutingPath,
                    MainWindowTitle = MpPrefViewModel.Instance.ApplicationName,
                    MainWindowIconBase64 = MpBase64Images.AppIcon
                };
            }
            return _thisAppInfo;
        }

        protected MpPortableProcessInfo RefreshRunningProcessLookup() {
            return GetActiveProcessInfo();
        }

        protected void CreateRunningProcessLookup() {

        }

        protected override string GetProcessTitle(nint handle) {
            throw new System.NotImplementedException();
        }

        protected override string GetProcessPath(nint handle) {
            throw new System.NotImplementedException();
        }

        protected override MpPortableProcessInfo GetProcessInfoByHandle(nint handle) {
            throw new System.NotImplementedException();
        }
    }
}

