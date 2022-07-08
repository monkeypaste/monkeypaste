using Avalonia.Controls;
using MonkeyPaste.Common;
using System.Linq;
using System.Text;
using Avalonia.Win32;
using Avalonia.Media.Imaging;

namespace MonkeyPaste.Avalonia {
    public class MpAvWrapper : MpIPlatformWrapper {
        public MpIProcessWatcher ProcessWatcher { get; set; }
        public MpICursor? Cursor { get; set; }
        public MpIDbInfo? DbInfo { get; set; }

        public MpIQueryInfo? QueryInfo { get; set; }
        public MpIIconBuilder? IconBuilder { get; set; }
        public MpIUrlBuilder? UrlBuilder { get; set; }
        public MpIAppBuilder? AppBuilder { get; set; }
        public MpICustomColorChooserMenu? CustomColorChooserMenu { get; set; }
        public MpIKeyboardInteractionService? KeyboardInteractionService { get; set; }
        public MpIGlobalTouch? GlobalTouch { get; set; }
        public MpIUiLocationFetcher? LocationFetcher { get; set; }
        public MpIPlatformResource? PlatformResource { get; set; }
        public MpIPlatformScreenInfoCollection? ScreenInfoCollection { get; set; }
        public MpIContextMenuCloser? ContextMenuCloser { get; set; }
        public MpIMainThreadMarshal? MainThreadMarshal { get; set; }
        public MpIStringTools? StringTools { get; set; }
        public MpIOsInfo? OsInfo { get; set; }
        public MpIPlatformDataObjectHelper? DataObjectHelper { get; set; }
        public MpINativeMessageBox? NativeMessageBox { get; set; }

        public MpIClipboardMonitor? ClipboardMonitor { get; set; }

        public MpIClipboardFormatDataHandlers? ClipboardData { get; set; }

        public MpIExternalPasteHandler? ExternalPasteHandler { get; set; }

        public MpIPlatformDataObjectRegistrar? DataObjectRegistrar { get; set; }

        public MpICopyItemBuilder? CopyItemBuilder { get; set; }
        public MpAvWrapper(Window w) {
            Cursor = new MpAvCursor();
            DbInfo = new MpAvDbInfo();
            QueryInfo = new MpAvQueryInfo();
            ProcessWatcher = new MpAvProcessWatcher().Watcher;

            IconBuilder = new MpAvIconBuilder().IconBuilder;

            //UrlBuilder = new MpUrlBuilder();
            AppBuilder = new MpAvAppBuilder();
            //CustomColorChooserMenu = new MpWpfCustomColorChooserMenu();
            //PlatformResource = new MpWpfResourceFetcher();
            //ContextMenuCloser = new MpWpfContextMenuCloser();
            MainThreadMarshal = new MpAvMainThreadMarshal();
            //StringTools = new MpWpfStringTools();
            OsInfo = new MpAvOsInfo();
            //NativeMessageBox = new MpWpfMessageBox();
            //DataObjectHelper = MpWpfDataObjectHelper.Instance;
            //ExternalPasteHandler = MpWpfDataObjectHelper.Instance;
            //CopyItemBuilder = new MpWpfCopyItemBuilder();
            ClipboardMonitor = new MpAvClipboardWatcher();
            //ClipboardData = MpClipboardHandlerCollectionViewModel.Instance;
            DataObjectRegistrar = ClipboardMonitor as MpIPlatformDataObjectRegistrar;
            ScreenInfoCollection = new MpAvScreenInfoCollection(w);
            
        }

    }
}
