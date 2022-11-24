using MonkeyPaste.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MonkeyPaste {
    public interface MpIProcessWatcher {
        IntPtr ThisAppHandle { get;}
        
        bool IsThisAppActive { get; }

        MpPortableProcessInfo LastProcessInfo { get; }


        ConcurrentDictionary<string,ObservableCollection<IntPtr>> RunningProcessLookup { get; }
        
        IntPtr GetParentHandleAtPoint(MpPoint poIntPtr);
        IntPtr GetLastActiveInstance(string path);
        bool IsHandleRunningProcess(IntPtr handle);
        string GetProcessPath(IntPtr handle);
        string ParseTitleForApplicationName(string windowTitle);
        string GetProcessApplicationName(IntPtr handle);
        string GetProcessTitle(IntPtr handle);
        IntPtr SetActiveProcess(IntPtr handle);

        event EventHandler<MpPortableProcessInfo> OnAppActivated;

        void StartWatcher();
        void StopWatcher();

    }
}
