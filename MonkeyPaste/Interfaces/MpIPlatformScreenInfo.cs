using MonkeyPaste.Common;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MonkeyPaste {
    public interface MpIPlatformScreenInfo {
        MpRect Bounds { get; set; }

        MpRect WorkArea { get; set; }
        bool IsPrimary { get; set; }

        double PixelDensity { get; set; }

        string Name { get; set; }
    }

    public interface MpIPlatformScreenInfoCollection {
        IEnumerable<MpIPlatformScreenInfo> Screens { get; set; }
    }
}
