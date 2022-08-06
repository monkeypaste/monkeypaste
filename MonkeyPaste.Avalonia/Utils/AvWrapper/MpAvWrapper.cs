using Avalonia.Controls;
using MonkeyPaste.Common;
using System.Linq;
using System.Text;
using Avalonia.Win32;
using Avalonia.Media.Imaging;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Reflection;

namespace MonkeyPaste.Avalonia {
    public class MpAvWrapper : MpIPlatformWrapper {
        private static MpAvWrapper _instance;
        public static MpAvWrapper Instance => _instance ?? (_instance = new MpAvWrapper());
        
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
        public async Task InitializeAsync(){
            string prefFileName = null;
            if (OperatingSystem.IsWindows()) {
                prefFileName = "pref_win.json";
            }
            if (OperatingSystem.IsLinux()) {
                prefFileName = "pref_x11.json";
            }
            if (OperatingSystem.IsMacOS()) {
                prefFileName = "pref_mac.json";
            }
            if(prefFileName == null) {
                throw new Exception("Unknown os");
            }
            string prefPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                prefFileName);

            await MpPrefViewModel.InitAsync(prefPath);
            MpPrefViewModel.Instance.MainWindowOrientation = "Bottom";

            Cursor = new MpAvCursor();
            DbInfo = new MpAvDbInfo();
            QueryInfo = new MpAvQueryInfo();
            ProcessWatcher = new MpAvProcessWatcher().Watcher;
            IconBuilder = new MpAvIconBuilder().IconBuilder;
            UrlBuilder = new MpUrlBuilder();
            AppBuilder = new MpAvAppBuilder();
            CustomColorChooserMenu = new MpAvCustomColorChooser();
            PlatformResource = new MpAvPlatformResource();
            ContextMenuCloser = new MpAvContextMenuCloser();
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
            ScreenInfoCollection = new MpAvScreenInfoCollection();
            
        }

    }
}
