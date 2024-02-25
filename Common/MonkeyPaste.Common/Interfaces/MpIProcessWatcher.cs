using System;
using System.Collections.Generic;

namespace MonkeyPaste.Common {

    public interface MpIProcessWatcher : MpIActionComponent {
        bool IsWatching { get; }
        bool IsProcessPathEqual(MpPortableProcessInfo p1, MpPortableProcessInfo p2);
        nint SetActiveProcess(MpPortableProcessInfo p);
        MpPortableProcessInfo GetProcessInfoFromScreenPoint(MpPoint pixelPoint);
        MpPortableProcessInfo GetProcessInfoFromHandle(nint handle);
        MpPortableProcessInfo LastProcessInfo { get; }
        MpPortableProcessInfo ThisAppProcessInfo { get; }
        IEnumerable<MpPortableProcessInfo> AllWindowProcessInfos { get; }

        event EventHandler<MpPortableProcessInfo> OnAppActivated;

        void StartWatcher();
        void StopWatcher();

    }
}
