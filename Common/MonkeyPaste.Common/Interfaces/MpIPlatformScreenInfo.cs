using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MonkeyPaste.Common {
    public interface MpIPlatformScreenInfo {
        MpRect Bounds { get; set; }

        MpRect WorkArea { get; set; }
        bool IsPrimary { get; set; }

        double Scaling { get; set; } // PixelDensity == PixelsPerDip

        void Rotate(double angle);
        bool IsEqual(MpIPlatformScreenInfo other);
    }

    public interface MpIPlatformScreenInfoCollection {
        ObservableCollection<MpIPlatformScreenInfo> Screens { get; }
        MpIPlatformScreenInfo Primary { get; }
        bool Refresh();
    }
}
