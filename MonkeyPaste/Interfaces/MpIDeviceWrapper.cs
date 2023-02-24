using MonkeyPaste.Common;

namespace MonkeyPaste {
    public interface MpIDeviceWrapper {
        MpIPlatformInfo PlatformInfo { get; }
        MpIPlatformScreenInfoCollection ScreenInfoCollection { get; }
    }
}
