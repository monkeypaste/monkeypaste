using System.Diagnostics;
using System.Linq;
using System.Text;
using MonkeyPaste;
using Xamarin.Forms.PlatformConfiguration;

namespace MpWpfApp {
    public class MpNativeWrapper : MpINativeInterfaceWrapper {
        public MpIconBuilder IconBuilder { private get; set; }
        public MpWpfDbInfo DbInfo { private get; set; }

        public MpIDbInfo GetDbInfo() {
            return DbInfo;
        }

        public MpIGlobalTouch GetGlobalTouch() {
            throw new System.NotImplementedException();
        }

        public MpIIconBuilder GetIconBuilder() {
            return IconBuilder;
        }

        public MpIKeyboardInteractionService GetKeyboardInteractionService() {
            throw new System.NotImplementedException();
        }

        public MpIUiLocationFetcher GetLocationFetcher() {
            throw new System.NotImplementedException();
        }
    }
}