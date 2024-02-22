using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System.IO;
//using Avalonia.Win32;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {

    public interface MpIDeviceWrapper {
        MpIClipboard DeviceClipboard { get; }
        MpIJsImporter JsImporter { get; }
        MpIPlatformInfo PlatformInfo { get; }
        MpIPlatformScreenInfoCollection ScreenInfoCollection { get; }
    }
    public class MpAvWrapper : MpIPlatformWrapper, MpAvICommonTools {

        #region Interfaces

        #region MpIPlatformWrapper Implementation

        #region Platform Services

        public MpIShare ShareTools { get; set; }

        #endregion

        #region Lazy Services

        public MpIPlatformScreenInfoCollection ScreenInfoCollection { get; set; }

        #endregion

        #region Startup Set Services 
        public MpICultureInfo UserCultureInfo { get; set; }
        public MpIUiStringToEnumConverter UiStrEnumConverter { get; set; }
        public MpIDebugBreakHelper DebugBreakHelper { get; set; }
        public MpIDefaultDataCreator DefaultDataCreator { get; set; }
        public MpIUserAgentProvider UserAgentProvider { get; set; }
        public MpIShutdownTools ShutdownHelper { get; set; }
        public MpIContentBuilder ContentBuilder { get; set; }

        public MpIDownKeyHelper KeyDownHelper { get; set; }
        public MpIContentQueryPage ContentQueryTools { get; set; }
        public MpITagQueryTools TagQueryTools { get; set; }
        public MpIContentViewLocator ContentViewLocator { get; set; }
        public MpIGlobalInputListener GlobalInputListener { get; set; }
        public MpIShortcutGestureLocator ShortcutGestureLocator { get; set; }

        #endregion 
        public MpISslInfo SslInfo { get; set; }
        public MpIWelcomeSetupInfo WelcomeSetupInfo { get; set; }
        public MpIUserDeviceInfo ThisDeviceInfo { get; set; }

        public MpINotificationBuilder NotificationBuilder { get; set; }
        public MpILoadOnLoginTools LoadOnLoginTools { get; set; }
        public MpIPlatformUserInfo PlatformUserInfo { get; set; }
        public MpIThisAppInfo ThisAppInfo { get; set; }
        public MpIMainThreadMarshal MainThreadMarshal { get; set; }
        public MpIColorQueryTools ColorQueryTools { get; set; }

        public MpIKeyConverterHub KeyConverter { get; set; }
        public MpIKeyStrokeSimulator KeyStrokeSimulator { get; set; }
        public MpIPlatformPathDialog NativePathDialog { get; set; }
        public MpIUserProvidedFileExts UserProvidedFileExts { get; set; }
        public MpIStartupState StartupState { get; set; }
        public MpIPlatformShorcuts PlatformShorcuts { get; set; }
        public MpIProcessWatcher ProcessWatcher { get; set; }
        public MpIDbInfo DbInfo { get; set; }
        public MpIFocusMonitor FocusMonitor { get; set; }
        public MpIDragProcessWatcher DragProcessWatcher { get; set; }
        public MpIDropProcessWatcher DropProcessWatcher { get; set; }
        public MpIQueryResultProvider Query { get; set; }
        public MpIIconBuilder IconBuilder { get; set; }
        public MpIUrlBuilder UrlBuilder { get; set; }
        public MpIAppBuilder AppBuilder { get; set; }
        public MpISourceRefTools SourceRefTools { get; set; }
        public MpITransactionReporter TransactionBuilder { get; set; }
        public MpICustomColorChooserMenuAsync CustomColorChooserMenuAsync { get; set; }
        public MpIKeyboardInteractionService KeyboardInteractionService { get; set; }
        public MpIPlatformResource PlatformResource { get; set; }
        public MpIStringTools StringTools { get; set; }
        public MpIPlatformInfo PlatformInfo { get; set; }
        public MpIPlatformDataObjectTools DataObjectTools { get; set; }
        public MpIPlatformMessageBox PlatformMessageBox { get; set; }

        public MpIClipboardMonitor ClipboardMonitor { get; set; }

        public MpIExternalPasteHandler ExternalPasteHandler { get; set; }

        public MpIPlatformDataObjectRegistrar DataObjectRegistrar { get; set; }

        private MpIClipboard _deviceClipboard;
        public MpIClipboard DeviceClipboard {
            get {
                if (_deviceClipboard == null) {
                    _deviceClipboard = new MpAvClipboardWrapper();
                }
                return _deviceClipboard;
            }
            set => _deviceClipboard = value;
        }
        #endregion

        #endregion

        #region Constructors
        public MpAvWrapper(MpIStartupState ss) {
            StartupState = ss;
        }
        #endregion

        #region Public Methods

        public async Task InitializeAsync() {
            ShutdownHelper = App.Instance;
            ThisAppInfo = new MpAvThisAppInfo();
            PlatformUserInfo = new MpAvPlatformUserInfo();

            if (MpDeviceWrapper.Instance != null) {
                PlatformInfo = MpDeviceWrapper.Instance.PlatformInfo;
                ScreenInfoCollection = MpDeviceWrapper.Instance.ScreenInfoCollection;
                IconBuilder = MpDeviceWrapper.Instance.IconBuilder;
                DeviceClipboard = MpDeviceWrapper.Instance.DeviceClipboard;
            } else {
                PlatformInfo = new MpAvPlatformInfo_desktop();
            }

            MpConsole.WriteLine($"Log Path: '{PlatformInfo.LogPath}'");
            MpConsole.WriteLine($"Storage Dir: '{PlatformInfo.StorageDir}'");
            MpConsole.WriteLine($"Executing Dir: '{PlatformInfo.ExecutingDir}'");
            string prefPath = Path.Combine(PlatformInfo.StorageDir, MpAvPrefViewModel.PREF_FILE_NAME);

            DbInfo = new MpAvDbInfo();
            await MpAvPrefViewModel.InitAsync(prefPath, DbInfo, PlatformInfo);

            DefaultDataCreator = new MpAvDefaultDataCreator();
            UserAgentProvider = MpAvPlainHtmlConverter.Instance;
            SslInfo = MpAvPrefViewModel.Instance;
            WelcomeSetupInfo = MpAvPrefViewModel.Instance;
            ThisDeviceInfo = MpAvPrefViewModel.Instance;
            ContentViewLocator = new MpAvContentViewLocator();
            ShareTools = new MpAvShare();
            NotificationBuilder = new MpAvNotificationBuilder();
            LoadOnLoginTools = new MpAvLoginLoadTools();
            ColorQueryTools = new MpAvColorQueryTools();
            NativePathDialog = new MpAvPathDialog();
            UserProvidedFileExts = MpAvPrefViewModel.Instance;
            Query = MpAvQueryViewModel.Parse(string.Empty);//MpPrefViewModel.Instance.LastQueryInfoJson);
            ProcessWatcher = new MpAvProcessWatcher();
            KeyConverter = new MpAvKeyConverter();

            UrlBuilder = new MpUrlBuilder();
            AppBuilder = new MpAvAppBuilder();
            SourceRefTools = new MpAvSourceRefTools();
            TransactionBuilder = new MpAvTransactionReporter();
            DebugBreakHelper = new MpAvDebugBreakHelper();

            FocusMonitor = MpAvFocusManager.Instance as MpIFocusMonitor;

            DragProcessWatcher = new MpAvDndProcessWatcher();
            DropProcessWatcher = DragProcessWatcher as MpIDropProcessWatcher;

            CustomColorChooserMenuAsync = new MpAvCustomColorChooserViewModel();

            KeyStrokeSimulator = new MpAvKeyStrokeSimulator();
            PlatformResource = new MpAvPlatformResource();

            MainThreadMarshal = new MpAvMainThreadMarshal();
            StringTools = new MpAvStringTools();
            PlatformMessageBox = new MpAvMessageBox();
            DataObjectTools = MpAvClipboardHandlerCollectionViewModel.Instance;
            ExternalPasteHandler = MpAvExternalPasteHandler.Instance;

            UiStrEnumConverter = new MpUiStringToEnumConverter();

            ClipboardMonitor = new MpAvClipboardWatcher();
            DataObjectRegistrar = ClipboardMonitor as MpIPlatformDataObjectRegistrar;

            PlatformShorcuts = new MpAvPlatformShortcuts();

            if (IconBuilder == null) {
                IconBuilder = new MpAvIconBuilder();
            }
#if !DESKTOP
            await MpDb.InitAsync();
#endif
            MpAvCommonTools.Init(this);
            MpAvCurrentCultureViewModel.Instance.Init();
            UserCultureInfo = MpAvCurrentCultureViewModel.Instance;
        }

        #endregion
    }

}
