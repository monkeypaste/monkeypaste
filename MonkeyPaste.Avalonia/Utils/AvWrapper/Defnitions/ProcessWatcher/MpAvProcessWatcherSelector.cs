using MonkeyPaste.Common;
using System;

namespace MonkeyPaste.Avalonia {
    public class MpAvProcessWatcherSelector {
        public MpIProcessWatcher Watcher { get; private set; }
        public MpAvProcessWatcherSelector() {
#if WINDOWS
            Watcher = new MpAvWin32ProcessWatcher();
#elif LINUX
            Watcher = new MpAvX11ProcessWatcher();
#elif MAC
            Watcher = new MpAvMacProcessWatcher();
#elif ANDROID
            Watcher = new MpAvAndroidProcessWatcher();
#endif
        }
    }
}
