using Avalonia.Input.Platform;
using MonkeyPaste;
using MonkeyPaste.Avalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace iosTest.iOS{
    public class MpAvIosWrapper : MpAvDeviceWrapper {

        #region Statics
        #endregion

        #region Interfaces

        #region MpIDeviceWrapper Implementation
        public override MpIPlatformInfo PlatformInfo { get; set; }
        public override MpIPlatformScreenInfoCollection ScreenInfoCollection { get; set; }
        public override MpIPathToPlatformIcon IconBuilder { get; set; }
        public override MpIClipboard DeviceClipboard { get; set; }

        #endregion


        #endregion

        #region Constructors
        #endregion
        #region Public Methods

        public override void CreateDeviceInstance(object args) {
            _instance = this;

            PlatformInfo = new MpAvIosPlatformInfo();
            ScreenInfoCollection = new MpAvIosScreenInfoCollection(new[] { new MpAvIosScreenInfo() });
            IconBuilder = new MpAvIosIconBuilder();
            DeviceClipboard = new MpAvIosClipboard();
            PlatformToastNotification = new MpAvIosToastNotification();
            DeviceWebViewHelper = new MpAvIosWebViewHelper();

            MpAvAssetMover.MoveAssets();
        }


        #endregion
    }
}
