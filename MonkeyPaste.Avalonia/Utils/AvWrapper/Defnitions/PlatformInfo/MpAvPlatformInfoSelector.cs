using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvPlatformInfoSelector {
        public MpIPlatformInfo PlatformInfo { get; private set; }
        public MpAvPlatformInfoSelector() : this(null) { }
        public MpAvPlatformInfoSelector(MpIPlatformInfo aux_info) {
            if (aux_info != null) {
                PlatformInfo = aux_info;
                return;
            }
            PlatformInfo = new MpAvPlatformInfo_desktop();
        }
    }
}
