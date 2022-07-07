using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    internal class MpAvProcessWatcher {
        public MpIProcessWatcher Watcher { get; private set; }
        public MpAvProcessWatcher() {
            //GtkTest.Test();

            if(OperatingSystem.IsWindows()) {
                Watcher = new MpWin32ProcessWatcher();
            } else if(OperatingSystem.IsLinux()) {
                Watcher = new MpX11ProcessWatcher();
            }
        }
    }
}
