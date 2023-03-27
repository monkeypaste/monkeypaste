using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia.Web {
    public class MpAvBrWrapper : MpDeviceWrapper, MpIDeviceWrapper {

        #region Interfaces

        #region MpIDeviceWrapper Implementation
        public override MpIPlatformInfo PlatformInfo { get; set; }
        public override MpIPlatformScreenInfoCollection ScreenInfoCollection { get; set; }

        #endregion


        #endregion

        #region Public Methods

        public override void CreateDeviceInstance(object args) {
            _instance = this;
        }
        public override async Task InitAsync(object args) {
            PlatformInfo = new MpAvBrPlatformInfo();
            JsImporter = MpAvNativeWebViewHost.Implementation as MpIJsImporter;
            await JsImporter.ImportAllAsync();
            ScreenInfoCollection = new MpAvBrScreenInfoCollection(new[] { new MpAvBrScreenInfo() });

        }
        #endregion
    }
}
