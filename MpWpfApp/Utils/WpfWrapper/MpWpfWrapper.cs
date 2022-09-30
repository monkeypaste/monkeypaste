using System.Diagnostics;
using System.Text;
using MonkeyPaste;
using MonkeyPaste.Common;

namespace MpWpfApp {
    public class MpWpfWrapper : MpIPlatformWrapper {
        public MpICursor Cursor { get; set; }
        MonkeyPaste.MpIProcessWatcher MpIPlatformWrapper.ProcessWatcher { get; set; }
        //public MpIProcessWatcher ProcessWatcher { get; set; }

        public MpIDbInfo DbInfo { get; set; }
        public MpIQueryInfo QueryInfo { get; set; }
        public MpIIconBuilder IconBuilder { get; set; }
        public MpIUrlBuilder UrlBuilder { get; set; }
        public MpIAppBuilder AppBuilder { get; set; }
        public MpICustomColorChooserMenu CustomColorChooserMenu { get; set; }
        public MpIKeyboardInteractionService KeyboardInteractionService { get; set; }
        public MpIGlobalTouch GlobalTouch { get; set; }
        public MpIUiLocationFetcher LocationFetcher { get; set; }
        public MpIPlatformResource PlatformResource { get; set; }
        public MpIPlatformScreenInfoCollection ScreenInfoCollection { get; set; }
        public MpIContextMenuCloser ContextMenuCloser { get; set; }
        public MpIMainThreadMarshal MainThreadMarshal { get; set; }
        public MpIStringTools StringTools { get; set; }
        public MpIOsInfo OsInfo { get; set; }
        public MpIPlatformDataObjectHelper DataObjectHelper { get; set; }
        public MpIPlatformDataObjectHelperAsync DataObjectHelperAsync { get; set; }
        public MpINativeMessageBox NativeMessageBox { get; set; }
        
        public MpIClipboardMonitor ClipboardMonitor { get; set; }

        public MpIClipboardFormatDataHandlers ClipboardData { get; set; }

        public MpIExternalPasteHandler ExternalPasteHandler { get; set; }

        public MpIPlatformDataObjectRegistrar DataObjectRegistrar { get; set; }

        public MpICopyItemBuilder CopyItemBuilder { get; set; }
        public MpWpfWrapper() {
            // NOTE ClipboardMonitor is set after bootstrapping
            // NOTE DataObjectRegistrar is set after bootstrapping
            Cursor = new MpWpfCursor();
            DbInfo = new MpWpfDbInfo();          
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