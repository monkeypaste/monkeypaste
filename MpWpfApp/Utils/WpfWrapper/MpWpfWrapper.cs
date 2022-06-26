using System.Diagnostics;
using System.Text;
using MonkeyPaste;
using MonkeyPaste.Common;

namespace MpWpfApp {
    public class MpWpfWrapper : MpIPlatformWrapper {
        public MpICursor Cursor { get; }
        public MpIDbInfo DbInfo { get; }
        public MpIPreferenceIO PreferenceIO { get; }
        public MpIQueryInfo QueryInfo { get; }
        public MpIIconBuilder IconBuilder { get; }
        public MpIUrlBuilder UrlBuilder { get; }
        public MpIAppBuilder AppBuilder { get; }
        public MpICustomColorChooserMenu CustomColorChooserMenu { get; }
        public MpIKeyboardInteractionService KeyboardInteractionService { get; }
        public MpIGlobalTouch GlobalTouch { get; }
        public MpIUiLocationFetcher LocationFetcher { get; }
        public MpIPlatformResource PlatformResource { get; }
        public MpIPlatformScreenInfoCollection ScreenInfoCollection { get; }
        public MpIContextMenuCloser ContextMenuCloser { get; }
        public MpIMainThreadMarshal MainThreadMarshal { get; }
        public MpIStringTools StringTools { get; }
        public MpIOsInfo OsInfo { get; }
        public MpIPlatformDataObjectHelper DataObjectHelper { get; }
        public MpINativeMessageBox NativeMessageBox { get; }
        
        public MpIClipboardMonitor ClipboardMonitor { get; }

        public MpIClipboardFormatDataHandlers ClipboardData { get;  }

        public MpIExternalPasteHandler ExternalPasteHandler { get; }

        public MpIPlatformDataObjectRegistrar DataObjectRegistrar { get; set; }

        public MpICopyItemBuilder CopyItemBuilder { get; }
        public MpWpfWrapper() {
            // NOTE ClipboardMonitor is set after bootstrapping
            // NOTE DataObjectRegistrar is set after bootstrapping

            Cursor = new MpWpfCursor();
            DbInfo = new MpWpfDbInfo();
            PreferenceIO = new MpWpfPreferences();            
            QueryInfo = new MpWpfQueryInfo();
            IconBuilder = new MpWpfIconBuilder();
            UrlBuilder = new MpUrlBuilder();
            AppBuilder = new MpWpfAppBuilder();
            CustomColorChooserMenu = new MpWpfCustomColorChooserMenu();
            PlatformResource = new MpWpfResourceFetcher();
            ContextMenuCloser = new MpWpfContextMenuCloser();
            MainThreadMarshal = new MpWpfMainThreadMarshal();
            StringTools = new MpWpfStringTools();
            OsInfo = new MpWpfOsInfo();
            NativeMessageBox = new MpWpfMessageBox();
            DataObjectHelper = MpWpfDataObjectHelper.Instance;
            ExternalPasteHandler = MpWpfDataObjectHelper.Instance;
            CopyItemBuilder = new MpWpfCopyItemBuilder();
            ClipboardMonitor = new MpWpfClipboardWatcher();
            ClipboardData = MpClipboardHandlerCollectionViewModel.Instance;
            DataObjectRegistrar = ClipboardMonitor as MpIPlatformDataObjectRegistrar;
            ScreenInfoCollection = new MpWpfScreenInfoCollection();            
        }

    }
}