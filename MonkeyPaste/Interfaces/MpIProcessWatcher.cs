using MonkeyPaste.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MonkeyPaste {
    public interface MpIProcessWatcher {
        IntPtr ThisAppHandle { get; }

        IntPtr LastHandle { get; }
        string LastProcessPath { get; }
        string LastMainWindowTitle { get; }

        ConcurrentDictionary<string,ObservableCollection<IntPtr>> RunningProcessLookup { get; }

        IntPtr GetParentHandleAtPoint(MpPoint poIntPtr);
        IntPtr GetLastActiveInstance(string path);
        bool IsHandleRunningProcess(IntPtr handle);
        string GetProcessPath(IntPtr handle);
        string GetProcessApplicationName(IntPtr handle);
        string GetProcessMainWindowTitle(IntPtr handle);
        void SetActiveProcess(IntPtr handle);

        event EventHandler<MpProcessActivatedEventArgs> OnAppActivated;

    }
}
