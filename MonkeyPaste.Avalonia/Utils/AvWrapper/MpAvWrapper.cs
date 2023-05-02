using MonkeyPaste.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
//using Avalonia.Win32;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvWrapper : MpIPlatformWrapper {

        #region Interfaces

        #region MpIPlatformWrapper Implementation

        #region Lazy Services

        public MpIPlatformScreenInfoCollection ScreenInfoCollection { get; set; }

        #endregion

        #region Bootstrapped Services (incomplete)

        public MpIDownKeyHelper KeyDownHelper { get; set; }
        public MpIContentQueryPage ContentQueryTools { get; set; }
        public MpITagQueryTools TagQueryTools { get; set; }
        public MpIStartupObjectLocator StartupObjectLocator { get; set; }

        public MpIContentViewLocator ContentViewLocator { get; set; }
        public MpIGlobalInputListener GlobalInputListener { get; set; }
        public MpIShortcutGestureLocator ShortcutGestureLocator { get; set; }

        #endregion

        public MpIMainThreadMarshal MainThreadMarshal { get; set; }
        public MpIColorQueryTools ColorQueryTools { get; set; }

        public MpIKeyConverterHub KeyConverter { get; set; }
        public MpIKeyStrokeSimulator KeyStrokeSimulator { get; set; }
        public MpINativePathDialog NativePathDialog { get; set; }
        public MpIUserProvidedFileExts UserProvidedFileExts { get; set; }
        public MpIStartupState StartupState { get; set; }
        public MpIPlatformShorcuts PlatformShorcuts { get; set; }
        public MpINotificationManager NotificationManager { get; set; }
        public MpIProcessWatcher ProcessWatcher { get; set; }
        public MpICursor Cursor { get; set; }
        public MpIDbInfo DbInfo { get; set; }
        public MpIFocusMonitor FocusMonitor { get; set; }
        public MpIDragProcessWatcher DragProcessWatcher { get; set; }
        public MpIDropProcessWatcher DropProcessWatcher { get; set; }
        public MpIApplicationCommandManager AppCommandManager { get; set; }
        public MpIQueryResultProvider Query { get; set; }
        public MpIIconBuilder IconBuilder { get; set; }
        public MpIUrlBuilder UrlBuilder { get; set; }
        public MpIAppBuilder AppBuilder { get; set; }
        public MpISourceRefTools SourceRefTools { get; set; }
        public MpITransactionReporter TransactionBuilder { get; set; }
        public MpICustomColorChooserMenuAsync CustomColorChooserMenuAsync { get; set; }
        public MpIKeyboardInteractionService KeyboardInteractionService { get; set; }
        public MpIPlatformResource PlatformResource { get; set; }
        public MpIContextMenuCloser ContextMenuCloser { get; set; }
        public MpIStringTools StringTools { get; set; }
        public MpIPlatformInfo PlatformInfo { get; set; }
        public MpIPlatformDataObjectHelperAsync DataObjectHelperAsync { get; set; }
        public MpINativeMessageBox NativeMessageBox { get; set; }

        public MpIClipboardMonitor ClipboardMonitor { get; set; }

        public MpIClipboardFormatDataHandlers ClipboardData { get; set; }

        public MpIExternalPasteHandler ExternalPasteHandler { get; set; }

        public MpIPlatformDataObjectRegistrar DataObjectRegistrar { get; set; }

        public MpICopyItemBuilder CopyItemBuilder { get; set; }

        #endregion

        #endregion

        #region Constructors
        public MpAvWrapper(MpIStartupState ss, MpIStartupObjectLocator sol) {
            StartupState = ss;
            StartupObjectLocator = sol;
        }
        #endregion

        #region Public Methods

        public async Task InitializeAsync() {
            if (MpDeviceWrapper.Instance != null) {
                PlatformInfo = MpDeviceWrapper.Instance.PlatformInfo;
                ScreenInfoCollection = MpDeviceWrapper.Instance.ScreenInfoCollection;
            } else {
                PlatformInfo = new MpAvPlatformInfo_desktop();
            }

            DbInfo = new MpAvDbInfo();

            string prefPath = Path.Combine(PlatformInfo.StorageDir, $"pref_{PlatformInfo.OsShortName}.json");

            if (App.Args.Any(x => x.ToLower() == App.BACKUP_DATA_ARG)) {
                // TODO move reset stuff to that backup folder
            }
            if (App.Args.Any(x => x.ToLower() == App.RESET_DATA_ARG)) {
                //Debugger.Break();

                // delete db, plugin cache, pref and pref.backup
                MpFileIo.DeleteFile(DbInfo.DbPath);
                MpFileIo.DeleteFile(DbInfo.DbPath + "-shm");
                MpFileIo.DeleteFile(DbInfo.DbPath + "-wal");

                MpFileIo.DeleteFileOrDirectory(MpPluginLoader.PluginManifestBackupFolderPath);
                MpFileIo.DeleteFile(prefPath);
                MpFileIo.DeleteFile($"{prefPath}.{MpPrefViewModel.PREF_BACKUP_PATH_EXT}");

                MpConsole.WriteLine("All data successfully deleted.");
            }

            await MpPrefViewModel.InitAsync(prefPath, DbInfo, PlatformInfo);

            ColorQueryTools = new MpAvColorQueryTools();
            NativePathDialog = new MpAvPathDialog();
            UserProvidedFileExts = MpPrefViewModel.Instance;
            Query = MpAvQueryViewModel.Parse(MpPrefViewModel.Instance.LastQueryInfoJson);
            ProcessWatcher = new MpAvProcessWatcherSelector().Watcher;
            KeyConverter = new MpAvKeyConverter();
            IconBuilder = new MpAvIconBuilder().IconBuilder;
            UrlBuilder = new MpUrlBuilder();
            AppBuilder = new MpAvAppBuilder();
            SourceRefTools = new MpAvSourceRefTools();
            TransactionBuilder = new MpAvTransactionReporter();

            FocusMonitor = MpAvFocusManager.Instance as MpIFocusMonitor;

            DragProcessWatcher = new MpAvDragProcessWatcher();
            DropProcessWatcher = new MpAvDropProcessWatcher();

            AppCommandManager = new MpAvApplicationCommandManager();

            CustomColorChooserMenuAsync = new MpAvCustomColorChooser();

            KeyStrokeSimulator = new MpAvKeyStrokeSimulator();
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

            NotificationManager = MpAvNotificationWindowManager.Instance;

            PlatformShorcuts = new MpAvPlatformShortcuts();
        }

        #endregion
    }
}
