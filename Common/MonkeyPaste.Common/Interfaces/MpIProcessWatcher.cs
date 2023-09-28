using System;
using System.Collections.Generic;

namespace MonkeyPaste.Common {

    public interface MpIProcessWatcher : MpIActionComponent {
        //nint ThisAppHandle { get; }
        bool IsWatching { get; }
        bool IsProcessPathEqual(nint h1, nint h2);
        MpPortableProcessInfo GetProcessInfoFromScreenPoint(MpPoint pixelPoint);
        MpPortableProcessInfo LastProcessInfo { get; }
        MpPortableProcessInfo ThisAppProcessInfo { get; }
        IEnumerable<MpPortableProcessInfo> AllWindowProcessInfos { get; }
        nint SetActiveProcess(nint handle);

        event EventHandler<MpPortableProcessInfo> OnAppActivated;

        void StartWatcher();
        void StopWatcher();

    }
}
