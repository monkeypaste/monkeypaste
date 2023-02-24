using Android.App;
using Android.Views;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

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
