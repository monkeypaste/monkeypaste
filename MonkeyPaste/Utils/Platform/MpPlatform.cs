using MonkeyPaste.Common;
using System.Threading.Tasks;

namespace MonkeyPaste {

    public static class MpPlatform {
        public static MpIPlatformWrapper Services { get; private set; }

        public static async Task InitAsync(MpIPlatformWrapper niw) {
            await Task.Delay(1);
            Services = niw;
            MpCommonTools.Init(niw);
        }


    }
}
