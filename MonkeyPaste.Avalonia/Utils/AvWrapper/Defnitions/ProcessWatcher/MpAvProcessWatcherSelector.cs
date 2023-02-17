using MonkeyPaste.Common;
using System;

namespace MonkeyPaste.Avalonia {
    public class MpAvProcessWatcherSelector {
        public MpIProcessWatcher Watcher { get; private set; }
        public MpAvProcessWatcherSelector() {
            if (OperatingSystem.IsWindows()) {
                Watcher = new MpAvWin32ProcessWatcher();
            } else if (OperatingSystem.IsLinux()) {
                Watcher = new MpAvX11ProcessWatcher();
            } else if (OperatingSystem.IsMacOS()) {
                Watcher = new MpAvMacProcessWatcher();
            } else if (OperatingSystem.IsAndroid()) {
                Watcher = new MpAvAndroidProcessWatcher();
            }
        }
    }
}
