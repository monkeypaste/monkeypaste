using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common;

namespace MonkeyPaste {

    public static class MpPlatformWrapper {
        public static MpIPlatformWrapper Services { get; private set; }

        public static async Task InitAsync(MpIPlatformWrapper niw) {
            await Task.Delay(1);
            Services = niw;
            MpCommonTools.Init(niw);
        }


    }
}
