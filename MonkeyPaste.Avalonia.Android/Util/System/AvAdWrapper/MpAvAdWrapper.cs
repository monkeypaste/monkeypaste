using Android.App;
using Android.Content.Res;
using AndroidX.Fragment.App.StrictMode;
using Avalonia.Input.Platform;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace MonkeyPaste.Avalonia.Android {
    public class MpAvAdWrapper : MpDeviceWrapper {

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
            if (args is not Activity activity) {
                return;
            }
            PlatformInfo = new MpAvAdPlatformInfo();
            ScreenInfoCollection = new MpAvAdScreenInfoCollection(new[] { new MpAvAdScreenInfo(activity) });
            IconBuilder = new MpAvAdIconBuilder();
            DeviceClipboard = new MpAvAdClipboard();

            MpAvAdAssetMover.MoveDats();

        }


        #endregion
    }
}
