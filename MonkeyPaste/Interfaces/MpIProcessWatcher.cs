using MonkeyPaste.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MonkeyPaste {
    public interface MpIProcessWatcher {
        IntPtr ThisAppHandle { get; set; }

        IntPtr LastHandle { get; }
        string LastProcessPath { get; }

        ConcurrentDictionary<string,List<IntPtr>> RunningProcessLookup { get; }

        IntPtr GetParentHandleAtPoint(MpPoint poIntPtr);
        IntPtr GetLastActiveInstance(string path);
        bool IsHandleRunningProcess(IntPtr handle);
        string GetProcessPath(IntPtr handle);
        string GetProcessApplicationName(IntPtr handle);
        string GetProcessMainWindowTitle(IntPtr handle);

        event EventHandler<MpProcessActivatedEventArgs> OnAppActivated;
    }
}
