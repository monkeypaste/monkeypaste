using MonkeyPaste.Common;
using SharpHook.Native;
using System;

namespace MonkeyPaste.Avalonia {
    public static class MpAvSharpHookExtensions {
        public static MpPoint GetScaledScreenPoint(this MouseEventData med) {

            //double scale = MpPlatform.Services.ScreenInfoCollection.PixelScaling;
            double scale = MpAvMainWindowViewModel.Instance.MainWindowScreen.Scaling;
            var unscaled_p = new MpPoint((double)med.X, (double)med.Y);
            var scaled_p = new MpPoint(Math.Max(0, (double)med.X / scale), Math.Max(0, (double)med.Y / scale));

            return scaled_p;
        }
    }
}
