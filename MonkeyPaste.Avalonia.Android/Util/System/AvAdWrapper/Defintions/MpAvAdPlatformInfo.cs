using Xamarin.Essentials;

namespace MonkeyPaste.Avalonia.Android {
    public class MpAvAdPlatformInfo : MpAvPlatformInfoBase {
        public override string OsMachineName =>
            DeviceInfo.Name;
        public override string OsVersion =>
            DeviceInfo.VersionString;
        public override bool IsTouchInputEnabled =>
            true;
    }
}
