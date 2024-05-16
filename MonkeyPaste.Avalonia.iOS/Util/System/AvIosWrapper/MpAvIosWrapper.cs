using Avalonia.Input.Platform;
using MonkeyPaste.Avalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace MonkeyPaste.Avalonia.iOS{
    public class MpAvIosWrapper : MpAvDeviceWrapper {

        #region Interfaces

        #region MpIDeviceWrapper Implementation
        public override MpIPlatformInfo PlatformInfo { get; set; }
        public override MpIPlatformScreenInfoCollection ScreenInfoCollection { get; set; }
        public override MpIPathToPlatformIcon IconBuilder { get; set; }
        public override MpIClipboard DeviceClipboard { get; set; }

        #endregion


        #endregion

        #region Public Methods

        public override void CreateDeviceInstance(object args) {
            _instance = this;

            PlatformInfo = new MpAvIosPlatformInfo();
            ScreenInfoCollection = new MpAvIosScreenInfoCollection(new[] { new MpAvIosScreenInfo() });
            IconBuilder = new MpAvIosIconBuilder();
            DeviceClipboard = new MpAvIosClipboard();
            //PlatformToastNotification = new MpAvAdToastNotification();
            //DeviceWebViewHelper = new MpAvAdWebViewHelper();

            MpAvIosAssetMover.MoveDats();

        }


        #endregion
    }
}
