using System.Diagnostics;
using System.Linq;
using System.Text;
using MonkeyPaste;
using Xamarin.Forms.PlatformConfiguration;

namespace MpWpfApp {
    public class MpWpfWrapper : MpINativeInterfaceWrapper {
        public MpIconBuilder IconBuilder { private get; set; }
        public MpWpfDbInfo DbInfo { private get; set; }
        public MpWpfPreferences WpfPreferences { private get; set; }
        public MpWpfQueryInfo QueryInfo { private get; set; }

        public MpWpfWrapper() {
            Init();
        }

        public void Init() {
            DbInfo = new MpWpfDbInfo();
            WpfPreferences = new MpWpfPreferences();
            QueryInfo = new MpWpfQueryInfo();
        }

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

        public MpIPreferenceIO GetPreferenceIO() {
            return WpfPreferences;
        }

        public MpIQueryInfo GetQueryInfo() {
            return QueryInfo;
        }
    }
}