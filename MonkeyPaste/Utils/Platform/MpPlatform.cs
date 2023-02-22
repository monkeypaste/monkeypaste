using MonkeyPaste.Common;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public static class MpPlatform {
        public static MpIPlatformWrapper Services { get; set; }

        public static async Task InitAsync(MpIPlatformWrapper niw) {
            MpCommonTools.Init(niw);
            Services = niw;
            await niw.InitializeAsync();
        }


    }
}
