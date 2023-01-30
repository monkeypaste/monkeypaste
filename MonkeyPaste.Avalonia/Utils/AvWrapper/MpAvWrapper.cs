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
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public class MpAvWrapper : MpIPlatformWrapper {

        #region Bootstrapped Services (incomplete)
        
        public MpIContentQueryTools ContentQueryTools { get; set; }
        public MpITagQueryTools TagQueryTools { get; set; }
        public MpIStartupObjectLocator StartupObjectLocator { get; set; }

        #endregion

        public MpIStartupState StartupState { get; set; }
        public MpIPlatformCompatibility PlatformCompatibility { get; set; }
        public MpIPlatformShorcuts PlatformShorcuts { get; set; }
        public MpINotificationManager NotificationManager { get; set; }
        public MpIProcessWatcher ProcessWatcher { get; set; }
        public MpICursor Cursor { get; set; }
        public MpIDbInfo DbInfo { get; set; }
        public MpIFocusMonitor FocusMonitor { get; set; }
        public MpIDragProcessWatcher DragProcessWatcher { get; set; }
        public MpIApplicationCommandManager AppCommandManager { get; set; }
        public MpIQueryResultProvider Query { get; set; }
        public MpIIconBuilder IconBuilder { get; set; }
        public MpIUrlBuilder UrlBuilder { get; set; }
        public MpIAppBuilder AppBuilder { get; set; }
        public MpISourceRefBuilder SourceRefBuilder { get; set; }
        public MpITransactionBuilder TransactionBuilder { get; set; }
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

            if(Program.Args != null) {
                if (Program.Args.Any(x => x.ToLower() == Program.BACKUP_DATA_ARG)) {
                    // TODO move reset stuff to that backup folder
                }
                if (Program.Args.Any(x => x.ToLower() == Program.RESET_DATA_ARG)) {

                    // TODO! Change tagids:
                    // RootGroudTagId=3
                    // HelpTagId=4
                    Debugger.Break();

                    // delete db, plugin cache, pref and pref.backup
                    MpFileIo.DeleteFile(DbInfo.DbPath);

                    MpFileIo.DeleteFileOrDirectory(MpPluginLoader.PluginManifestBackupFolderPath);
                    MpFileIo.DeleteFile(prefPath);
                    MpFileIo.DeleteFile($"{prefPath}.{MpPrefViewModel.PREF_BACKUP_PATH_EXT}");

                    MpConsole.WriteLine("All data successfully deleted.");
                }
            }
            
            await MpPrefViewModel.InitAsync(prefPath, DbInfo, OsInfo);

            Query = MpAvQueryInfoViewModel.Parse(MpPrefViewModel.Instance.LastQueryInfoJson);
            ProcessWatcher = new MpAvProcessWatcherSelector().Watcher;

            IconBuilder = new MpAvIconBuilder().IconBuilder;
            UrlBuilder = new MpUrlBuilder();
            AppBuilder = new MpAvAppBuilder();
            SourceRefBuilder = new MpAvSourceRefBuilder();
            TransactionBuilder = new MpAvTransactionBuilder();

            FocusMonitor = MpAvFocusManager.Instance as MpIFocusMonitor;

            DragProcessWatcher = new MpAvDragProcessWatcher();

            AppCommandManager = new MpAvApplicationCommandManager();

            CustomColorChooserMenuAsync = new MpAvCustomColorChooser();

            PlatformCompatibility = new MpAvPlatformCompatibility();
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
            NotificationManager = MpAvNotificationWindowManager.Instance;

            PlatformShorcuts = new MpAvPlatformShortcuts();
        }

    }
}
