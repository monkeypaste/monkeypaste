using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MonkeyPaste.Common {
    public interface MpIProcessWatcher : MpIActionComponent {
        int PollIntervalMs { get; }
        IntPtr ThisAppHandle { get; }
        bool IsWatching { get; }

        bool IsThisAppActive { get; }
        MpPortableProcessInfo GetActiveProcessInfo();
        MpPortableProcessInfo LastProcessInfo { get; }
        MpPortableProcessInfo FileSystemProcessInfo { get; }

        ConcurrentDictionary<string, ObservableCollection<IntPtr>> RunningProcessLookup { get; }

        //bool IsParentHandle(IntPtr parentHandle, IntPtr handle);
        IntPtr GetParentHandleAtPoint(MpPoint poIntPtr);
        IntPtr GetLastActiveInstance(string path);
        bool IsHandleRunningProcess(IntPtr handle);
        string GetProcessPath(IntPtr handle);
        Process GetProcess(object handleIdOrTitle);
        bool IsAdmin(object handleIdOrTitle);
        ProcessWindowStyle GetWindowStyle(object handleIdOrTitle);
        string ParseTitleForApplicationName(string windowTitle);
        string GetProcessApplicationName(IntPtr handle);
        string GetProcessTitle(IntPtr handle);
        IntPtr SetActiveProcess(IntPtr handle);
        IntPtr SetActiveProcess(IntPtr handle, ProcessWindowStyle windowStyle);

        event EventHandler<MpPortableProcessInfo> OnAppActivated;

        void StartWatcher();
        void StopWatcher();

    }
}
