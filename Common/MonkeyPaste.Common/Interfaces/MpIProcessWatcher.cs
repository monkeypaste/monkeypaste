using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MonkeyPaste.Common {

    public interface MpIProcessWatcher : MpIActionComponent {
        //nint ThisAppHandle { get; }
        bool IsWatching { get; }

        MpPortableProcessInfo GetProcessInfoFromScreenPoint(MpPoint screenPoint);
        MpPortableProcessInfo LastProcessInfo { get; }
        MpPortableProcessInfo ThisAppProcessInfo { get; }
        nint SetActiveProcess(nint handle);

        event EventHandler<MpPortableProcessInfo> OnAppActivated;

        void StartWatcher();
        void StopWatcher();

    }
}
