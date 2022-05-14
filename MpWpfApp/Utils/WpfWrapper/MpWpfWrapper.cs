using System.Diagnostics;
using System.Linq;
using System.Text;
using MonkeyPaste;
using Xamarin.Forms.PlatformConfiguration;
using System.Threading.Tasks;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpWpfWrapper : MpIPlatformWrapper {
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
        public MpIStringTools StringTools { get; }
        public MpIOsInfo OsInfo { get; }
        public MpIPlatformDataObjectHelper DataObjectHelper { get; }
        public MpINativeMessageBox NativeMessageBox { get; }
        
        public MpIClipboardMonitor ClipboardMonitor { get; set; }
        public MpWpfWrapper() {
            // NOTE ClipboardMonitor is set after bootstrapping

            Cursor = new MpWpfCursor();
            DbInfo = new MpWpfDbInfo();
            PreferenceIO = new MpWpfPreferences();
            QueryInfo = new MpWpfQueryInfo();
            IconBuilder = new MpWpfIconBuilder();
            CustomColorChooserMenu = new MpWpfCustomColorChooserMenu();
            NativeResource = new MpWpfResourceFetcher();
            ContextMenuCloser = new MpWpfContextMenuCloser();
            MainThreadMarshal = new MpWpfMainThreadMarshal();
            StringTools = new MpWpfStringTools();
            OsInfo = new MpWpfOsInfo();
            NativeMessageBox = new MpWpfMessageBox();
            DataObjectHelper = new MpWpfDataObjectHelper();
        }

        
    }
}