using System.Diagnostics;
using System.Linq;
using System.Text;
using MonkeyPaste;
using Xamarin.Forms.PlatformConfiguration;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpWpfWrapper : MpINativeInterfaceWrapper {
        public MpWpfDbInfo DbInfo { private get; set; }
        public MpWpfPreferences WpfPreferences { private get; set; }
        public MpWpfQueryInfo QueryInfo { private get; set; }
        public MpWpfIconBuilder IconBuilder { private get; set; }
        public MpWpfCustomColorChooserMenu CustomColorChooserMenu { private get; set; }

        public MpWpfWrapper() {
            DbInfo = new MpWpfDbInfo();
            WpfPreferences = new MpWpfPreferences();
            QueryInfo = new MpWpfQueryInfo();
            IconBuilder = new MpWpfIconBuilder();
            CustomColorChooserMenu = new MpWpfCustomColorChooserMenu();
        }


        public MpIDbInfo GetDbInfo() {
            return DbInfo;
        }

        public MpIGlobalTouch GetGlobalTouch() {
            throw new System.NotImplementedException();
        }

        public MpIconBuilderBase GetIconBuilder() {
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

        public MpICustomColorChooserMenu GetCustomColorChooserMenu() {
            return CustomColorChooserMenu;
        }
    }
}