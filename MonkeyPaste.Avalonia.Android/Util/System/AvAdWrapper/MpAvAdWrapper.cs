using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia.Android {
    public class MpAvAdWrapper : MpDeviceWrapper, MpIDeviceWrapper {

        #region Interfaces

        #region MpIDeviceWrapper Implementation

        private MpIPlatformInfo _platformInfo = new MpAvAdPlatformInfo();
        public override MpIPlatformInfo PlatformInfo =>
            _platformInfo;

        #endregion


        #endregion

        #region Public Methods

        public override void CreateDeviceInstance() {
            _instance = this;
        }
        #endregion
    }
}
