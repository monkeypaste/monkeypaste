using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace MonkeyPaste.Avalonia.Android {
    public class MpAvAdPlatformInfo : MpAvPlatformInfoBase {
        public override string OsMachineName =>
            DeviceInfo.Name;
        public override string OsVersionInfo =>
            DeviceInfo.VersionString;
        public override bool IsTouchInputEnabled =>
            true;
    }
}
