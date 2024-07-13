using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvDeviceWrapper : MpIDeviceWrapper {
        #region Statics
        protected static MpAvDeviceWrapper _instance;
        public static MpAvDeviceWrapper Instance =>
            _instance;

        #endregion

        #region Interfaces

        #region MpIDeviceWrapper Implementation
        public MpAvIDeviceWebViewHelper DeviceWebViewHelper { get; set; }
        public MpIPlatformToastNotification PlatformToastNotification { get; set; }
        public MpIJsImporter JsImporter { get; protected set; }
        public abstract MpIPlatformInfo PlatformInfo { get; set; }
        public abstract MpIPlatformScreenInfoCollection ScreenInfoCollection { get; set; }

        public abstract MpIPathToPlatformIcon IconBuilder { get; set; }
        public abstract MpIClipboard DeviceClipboard { get; set; }

        #endregion
        #endregion

        #region Public Methods

        public abstract void CreateDeviceInstance(object args);

        public virtual async Task InitAsync(object args) { await Task.Delay(1); }


        #endregion


    }
}
