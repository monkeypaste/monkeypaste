using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public class MpAvAndroidProcessWatcher : MpAvProcessWatcherBase {
        public override nint GetParentHandleAtPoint(MpPoint poIntPtr) {
            return App.Current.GetMainWindowHandle();
        }

        public override nint SetActiveProcess(nint handle) {
            return handle;
        }

        public override nint SetActiveProcess(nint handle, ProcessWindowStyle windowStyle) {
            return handle;
        }

        public override bool IsAdmin(object handleIdOrTitle) {
            return false;
        }

        public override ProcessWindowStyle GetWindowStyle(object handleIdOrTitle) {
            return ProcessWindowStyle.Maximized;
        }

        private MpPortableProcessInfo _thisAppInfo;
        public override MpPortableProcessInfo GetActiveProcessInfo() {
            if (_thisAppInfo == null) {
                _thisAppInfo = new MpPortableProcessInfo() {
                    Handle = App.Current.GetMainWindowHandle(),
                    ProcessPath = MpCommonHelpers.GetExecutingPath(),
                    MainWindowTitle = MpPrefViewModel.Instance.ApplicationName,
                    MainWindowIconBase64 = MpBase64Images.AppIcon
                };
            }
            return _thisAppInfo;
        }

        protected override MpPortableProcessInfo RefreshRunningProcessLookup() {
            return GetActiveProcessInfo();
        }

        protected override void CreateRunningProcessLookup() {

        }
    }
}

