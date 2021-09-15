using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MonkeyPaste {
    public static class MpMainDisplayInfo  {
        public static DisplayInfo MainDisplayInfo;
        public static void Init() {
            Device.BeginInvokeOnMainThread(() => {
                MainDisplayInfo = Xamarin.Essentials.DeviceDisplay.MainDisplayInfo;
            });
        }
    }
}
