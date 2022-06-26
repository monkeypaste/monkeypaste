using MonkeyPaste.Common;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MonkeyPaste {
    public interface MpIPlatformScreenInfo {
        MpRect Bounds { get; }

        MpRect WorkArea { get; }
        bool IsPrimary { get; }

        string Name { get; }
    }

    public interface MpIPlatformScreenInfoCollection {
        IEnumerable<MpIPlatformScreenInfo> Screens { get; }
    }
}
