using System.Diagnostics;
using System.Linq;
using System.Text;
using MonkeyPaste;
using Xamarin.Forms.PlatformConfiguration;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpWpfWrapper : MpINativeInterfaceWrapper {
        public MpICursor Cursor { get; }
        public MpIDbInfo DbInfo { get; }
        public MpIPreferenceIO PreferenceIO { get; }
        public MpIQueryInfo QueryInfo { get; }
        public MpIIconBuilder IconBuilder { get; }
        public MpICustomColorChooserMenu CustomColorChooserMenu { get; }
        public MpIKeyboardInteractionService KeyboardInteractionService { get; }
        public MpIGlobalTouch GlobalTouch { get; }
        public MpIUiLocationFetcher LocationFetcher { get; }
        public MpINativeResource NativeResource { get; }
        public MpIContextMenuCloser ContextMenuCloser { get; }
        public MpIMainThreadMarshal MainThreadMarshal { get; }

        public MpWpfWrapper() {
            Cursor = new MpWpfCursor();
            DbInfo = new MpWpfDbInfo();
            PreferenceIO = new MpWpfPreferences();
            QueryInfo = new MpWpfQueryInfo();
            IconBuilder = new MpWpfIconBuilder();
            CustomColorChooserMenu = new MpWpfCustomColorChooserMenu();
            NativeResource = new MpWpfResourceFetcher();
            ContextMenuCloser = new MpWpfContextMenuCloser();
            MainThreadMarshal = new MpWpfMainThreadMarshal();
        }

        
    }
}