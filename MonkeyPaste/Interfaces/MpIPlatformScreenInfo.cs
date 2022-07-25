using MonkeyPaste.Common;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MonkeyPaste {
    public interface MpIPlatformScreenInfo {
        MpRect Bounds { get; set; }

        MpRect WorkArea { get; set; }
        bool IsPrimary { get; set; }

        double PixelDensity { get; set; } // PixelDensity == PixelsPerDip

        MpPoint PixelsPerInch { get; } // PixelsPerInch == 96 * PixelDensity
        string Name { get; set; }
    }

    public interface MpIPlatformScreenInfoCollection {
        double PixelScaling { get; }
        IEnumerable<MpIPlatformScreenInfo> Screens { get; }
    }
}
