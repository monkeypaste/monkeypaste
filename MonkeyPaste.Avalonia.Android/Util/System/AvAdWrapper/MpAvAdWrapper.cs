using Android.App;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia.Android {
    public class MpAvAdWrapper : MpDeviceWrapper, MpIDeviceWrapper {

        #region Interfaces

        #region MpIDeviceWrapper Implementation
        public override MpIPlatformInfo PlatformInfo { get; set; }
        public override MpIPlatformScreenInfoCollection ScreenInfoCollection { get; set; }

        #endregion


        #endregion

        #region Public Methods

        public override void CreateDeviceInstance(object args) {
            PlatformInfo = new MpAvAdPlatformInfo();
            ScreenInfoCollection = new MpAvAdScreenInfoCollection(new[] { new MpAvAdScreenInfo(args as Activity) });
            _instance = this;
        }
        #endregion
    }
}
