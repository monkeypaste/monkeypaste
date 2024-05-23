using Microsoft.Maui.Devices;
using MonkeyPaste.Avalonia;

namespace MonkeyPaste.Avalonia.iOS{
    public class MpAvIosPlatformInfo : MpAvPlatformInfoBase {
        public override string OsMachineName =>
            DeviceInfo.Name;
        public override string OsVersion =>
            DeviceInfo.VersionString;
        public override bool IsTouchInputEnabled =>
            true;
    }
}
