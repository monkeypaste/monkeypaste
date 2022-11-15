using Avalonia.Controls;
using System.Linq;
using System.Text;
//using Avalonia.Win32;
using Avalonia.Media.Imaging;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Reflection;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvWrapper : MpIPlatformWrapper {
        //private static MpAvWrapper _instance;
        //public static MpAvWrapper Instance => _instance  (_instance = new MpAvWrapper());

        public MpINotificationBalloonView NotificationView { get; set; }
        public MpIProcessWatcher ProcessWatcher { get; set; }
        public MpICursor Cursor { get; set; }
        public MpIDbInfo DbInfo { get; set; }

        public MpIQueryInfo QueryInfo { get; set; }
        public MpIIconBuilder IconBuilder { get; set; }
        public MpIUrlBuilder UrlBuilder { get; set; }
        public MpIAppBuilder AppBuilder { get; set; }
        public MpICustomColorChooserMenu CustomColorChooserMenu { get; set; }
        public MpICustomColorChooserMenuAsync CustomColorChooserMenuAsync { get; set; }
        public MpIKeyboardInteractionService KeyboardInteractionService { get; set; }
        public MpIGlobalTouch GlobalTouch { get; set; }
        public MpIUiLocationFetcher LocationFetcher { get; set; }
        public MpIPlatformResource PlatformResource { get; set; }
        public MpIPlatformScreenInfoCollection ScreenInfoCollection { get; set; }
        public MpIContextMenuCloser ContextMenuCloser { get; set; }
        public MpIMainThreadMarshal MainThreadMarshal { get; set; }
        public MpIStringTools StringTools { get; set; }
        public MpIOsInfo OsInfo { get; set; }
        public MpIPlatformDataObjectHelperAsync DataObjectHelperAsync { get; set; }
        public MpINativeMessageBox NativeMessageBox { get; set; }

        public MpIClipboardMonitor ClipboardMonitor { get; set; }

        public MpIClipboardFormatDataHandlers ClipboardData { get; set; }

        public MpIExternalPasteHandler ExternalPasteHandler { get; set; }

        public MpIPlatformDataObjectRegistrar DataObjectRegistrar { get; set; }

        public MpICopyItemBuilder CopyItemBuilder { get; set; }
        public async Task InitializeAsync() {
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

            DbInfo = new MpAvDbInfo();
            OsInfo = new MpAvOsInfo();
            await MpPrefViewModel.InitAsync(prefPath, DbInfo, OsInfo);

            MpAvQueryInfoViewModel.Init(MpPrefViewModel.Instance.LastQueryInfoJson);
            QueryInfo = MpAvQueryInfoViewModel.Current;
            ProcessWatcher = new MpAvProcessWatcherSelector().Watcher;
            IconBuilder = new MpAvIconBuilder().IconBuilder;
            UrlBuilder = new MpUrlBuilder();
            AppBuilder = new MpAvAppBuilder();
            
            CustomColorChooserMenu = new MpAvCustomColorChooser();
            CustomColorChooserMenuAsync = CustomColorChooserMenu as MpICustomColorChooserMenuAsync;

            PlatformResource = new MpAvPlatformResource();
            Cursor = new MpAvCursor((MpAvPlatformResource)PlatformResource);
            ContextMenuCloser = new MpAvContextMenuCloser();
            MainThreadMarshal = new MpAvMainThreadMarshal();
            StringTools = new MpAvStringTools();
            NativeMessageBox = new MpAvMessageBox();
            DataObjectHelperAsync = MpAvClipboardHandlerCollectionViewModel.Instance;
            ExternalPasteHandler = MpAvExternalPasteHandler.Instance;
            CopyItemBuilder = new MpAvCopyItemBuilder();

            ClipboardMonitor = new MpAvClipboardWatcher();
            DataObjectRegistrar = ClipboardMonitor as MpIPlatformDataObjectRegistrar;

            ScreenInfoCollection = new MpAvScreenInfoCollection();
            NotificationView = MpAvNotificationWindow.Instance;
        }

    }
}
