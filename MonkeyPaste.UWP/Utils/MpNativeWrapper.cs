using System.Diagnostics;
using System.Linq;
using System.Text;
using MonkeyPaste;
using Xamarin.Forms.Platform.UWP;
using Xamarin.Forms.PlatformConfiguration;

namespace MonkeyPaste.UWP {
    public class MpNativeWrapper : MpINativeInterfaceWrapper {
        public MpKeyboardInteractionService KeyboardService { private get; set; }
        public MpGlobalTouch TouchService { private get; set; }
        public MpUiLocationFetcher UiLocationFetcher { private get; set; }
        public MpDbFilePath_Uwp DbInfo { private get; set; }

        public MpIDbInfo GetDbInfo() {
            return DbInfo;
        }

        public MpIGlobalTouch GetGlobalTouch() {
            return TouchService;
        }

        public MpIIconBuilder GetIconBuilder() {
            return null;
        }

        public MpIKeyboardInteractionService GetKeyboardInteractionService() {
            return KeyboardService;
        }

        public MpIUiLocationFetcher GetLocationFetcher() {
            return UiLocationFetcher;
        }
    }
}