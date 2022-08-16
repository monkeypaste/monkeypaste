using MonkeyPaste.Common;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MonkeyPaste {
    public class MpPrefViewModel : MpViewModelBase, MpIJsonObject {
        #region Private
        [JsonIgnore]
        private object _lock = new object();

        [JsonIgnore]
        private static string _prefPath;

        [JsonIgnore]
        private static MpIDbInfo _dbInfo;

        [JsonIgnore]
        private static MpIOsInfo _osInfo;
        #endregion

        #region Constants
        //[JsonIgnore]
        //public const string PREFERENCES_FILE_NAME = "Pref.json";
        [JsonIgnore]
        public const string STRING_ARRAY_SPLIT_TOKEN = "<&>SPLIT</&>";

        [JsonIgnore]
        public const bool UseEncryption = false;

        #endregion

        #region Statics
        //[JsonIgnore]
        //private static MpJsonPreferenceIO _instance;
        [JsonIgnore]
        public static MpPrefViewModel Instance { get; private set; } //=> _instance ?? (new MpJsonPreferenceIO());

        #endregion


        #region Properties

        #region Property Reflection Referencer

        [JsonIgnore]
        public object this[string propertyName] {
            get {
                // probably faster without reflection:
                // like:  return Properties.Settings.Default.PropertyValues[propertyName] 
                // instead of the following
                Type myType = typeof(MpPrefViewModel);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                if (myPropInfo == null) {
                    throw new Exception("Unable to find property: " + propertyName);
                }
                return myPropInfo.GetValue(this, null);
            }
            set {
                Type myType = typeof(MpPrefViewModel);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);
            }
        }

        #endregion

        #region Application Properties

        public string ThisAppName => "Monkey Paste";


        public string ThisAppPath => Assembly.GetExecutingAssembly().Location;

        #region Encyption
        public string SslAlgorithm { get; set; } = "SHA256WITHRSA";
        public string SslCASubject { get; set; } = "CN=MPCA";
        public string SslCertSubject { get; set; } = "CN=127.0.01";
        #endregion

        public MpUserDeviceType ThisDeviceType { get; set; } 

        public string LocalStoragePath => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        #region Db

        

        #region Sync
        public string SyncCertFolderPath => Path.Combine(LocalStoragePath, "SyncCerts");

        public string SyncCaPath => Path.Combine(SyncCertFolderPath, @"MPCA.cert");

        public string SyncCertPath => Path.Combine(SyncCertFolderPath, @"MPSC.cert");

        public string SyncServerProtocol => @"https://";

        public string SyncServerHostNameOrIp => "monkeypaste.com";

        public int SyncServerPort { get; set; } = 44376;

        public string SyncServerEndpoint => $"{SyncServerProtocol}{SyncServerHostNameOrIp}:{SyncServerPort}";

        #endregion
        #endregion

        #region Appearance
        public double LogWindowHeightRatio {
            get {
                return 0.35;
            }
        }

        public double MainWindowStartHeight {
            get {
                return 10000;
            }
        }
        #endregion

        #region Resources

        public string AbsoluteResourcesPath {
            get {
                return @"pack://application:,,,/Resources";
            }
        }

        public int MaxFilePathCharCount {
            get {
                return 260;
            }
        }

        #endregion

        #region Drag & Drop
        public string CompositeItemDragDropFormatName {
            get {
                return "CompositeItemDragDropFormat";
            }
        }

        public string ClipTileDragDropFormatName {
            get {
                return "MpClipDragDropFormat";
            }
        }
        #endregion

        #region Experience
        public int ShowMainWindowAnimationMilliseconds {
            get {
                return 500;
            }
        }

        public int HideMainWindowAnimationMilliseconds {
            get {
                return 250;
            }
        }

        public int SearchBoxTypingDelayInMilliseconds {
            get {
                return 500;
            }
        }

        public string NotificationCopySound1Path {
            get {
                return @"Sounds/Ting.wav";
            }
        }

        public int ShowMainWindowMouseHitZoneHeight {
            get {
                return 5;
            }
        }

        public string DefaultCultureInfoName {
            get {
                return @"en-US";
            }
        }

        public string SearchPlaceHolderText {
            get {
                return @"Search...";
            }
        }

        public string ApplicationName {
            get {
                return @"Monkey Paste";
            }
        }
        #endregion

        #region REST

        public string CurrencyConverterFreeApiKey {
            get {
                return @"897d0d9538155ebeaff7";
            }
        }

        public string AzureCognitiveServicesKey {
            get {
                return "b455280a2c66456e926b66a1e6656ce3";
            }
        }

        public string AzureTextAnalyticsKey {
            get {
                return "ec769ed641ac48ed86b38363e67e824b";
            }
        }

        public string AzureTextAnalyticsEndpoint {
            get {
                return @"https://mp-azure-text-analytics-services-resource-instance.cognitiveservices.azure.com/";
            }
        }

        public string AzureCognitiveServicesEndpoint {
            get {
                return @"https://mp-azure-cognitive-services-resource-instance.cognitiveservices.azure.com/";
            }
        }

        public string BitlyApiToken {
            get {
                return @"f6035b9ed05ac82b42d4853c984e34a4f1ba05d8";
            }
        }

        public string RestfulOpenAiApiKey {
            get {
                return @"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";
            }
        }

        public string DomainFavIconEndpoint {
            get {
                return @"https://www.google.com/s2/favicons?https://www.google.com/s2/favicons?sz=64&domain_url=";
            }
        }
        #endregion

        #region Settings 
        public string AutoSelectionElementTag {
            get {
                return "AutoSelectionElement";
            }
        }
        public int MaxCommandLineArgumentLength {
            get {
                return 1024;
            }
        }


        public int MaxQrCodeCharLength {
            get {
                return 4296;
            }
        }

        public int MaxTemplateTextLength {
            get {
                return 10;
            }
        }


        #endregion

        #endregion

        #region User Properties          

        public string RecentFindTexts { get; set; } = string.Empty;

        public string RecentReplaceTexts { get; set; } = string.Empty;

        public string RecentSearchTexts { get; set; } = string.Empty;

        public int MaxRecentTextsCount { get; set; } = 8;

        public string IgnoredProcessNames { get; set; } = string.Empty;
        public string DoNotShowAgainNotificationIdCsvStr { get; set; } = string.Empty;

        public string AppStorageFilePath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public string MainWindowOrientation { get; set; } = "Bottom";

        public string MainWindowDisplayType { get; set; } = "Primary";

        public double MainWindowInitialWidth { get; set; } = 0;
        public double MainWindowInitialHeight { get; set; } = 0;
        

        public DateTime StartupDateTime { get; set; } = DateTime.MinValue;

        public string UserCultureInfoName { get; set; } = @"en-US";

        public int UniqueContentItemIdx { get; set; } = 0;

        public string ThisDeviceGuid { get; set; } = System.Guid.NewGuid().ToString();

        public bool ShowMainWindowOnDragToScreenTop { get; set; } = true;

        public int ThisAppSourceId { get; set; } = 0;

        public int ThisOsFileManagerSourceId { get; set; } = 0;

        #region Encrytion

        public string SslPrivateKey { get; set; } = string.Empty;

        public string SslPublicKey { get; set; } = string.Empty;

        public DateTime SslCertExpirationDateTime { get; set; } = DateTime.UtcNow.AddDays(-1);

        public string FallbackProcessPath { get; set; } = @"C:\WINDOWS\Explorer.EXE";
        #endregion

        #region Db

        public bool EncryptDb { get; set; } = true;

        public string DbPassword { get; set; } = MpPasswordGenerator.GetRandomPassword();
        #endregion

        #region Sync
        public int SyncPort { get; set; } = 11000;
        #endregion

        #region REST

        public int RestfulLinkMinificationMaxCount { get; set; } = 5;

        public int RestfulDictionaryDefinitionMaxCount { get; set; } = 5;

        public int RestfulTranslationMaxCount { get; set; } = 5;
        public int RestfulCurrencyConversionMaxCount { get; set; } = 5;

        public int RestfulLinkMinificationCount { get; set; } = 0;

        public int RestfulDictionaryDefinitionCount { get; set; } = 0;

        public int RestfulCurrencyConversionCount { get; set; } = 0;

        public int RestfulTranslationCount { get; set; } = 0;

        public DateTime RestfulBillingDate { get; set; } = DateTime.UtcNow;

        public int RestfulOpenAiCount { get; set; } = 0;

        public int RestfulOpenAiMaxCount { get; set; } = 5;
        #endregion

        #region Appearance

        public double MainWindowOpacity { get; set; } = 0.7;

        public string UserCustomColorIdxArray { get; set; } = "0";

        public string ThemeClipTileBackgroundColor { get; set; } = "#FFFFF";

        public string HighlightFocusedHexColorString { get; set; } = "#FFC0CB";

        public string HighlightColorHexString { get; set; } = "#FFFF00";

        public string ClipTileBackgroundColor { get; set; } = "#FFFFFF";

        public string DefaultFontFamily { get; set; } = "Consolas";

        public double DefaultFontSize { get; set; } = 12.0d;

        public string SpeechSynthVoiceName { get; set; } = "Zira";

        public bool IgnoreNewDuplicates { get; set; } = true;

        public int MaxRecentClipItems { get; set; } = 25;

        public int NotificationBalloonVisibilityTimeMs { get; set; } = 3000;

        public int NotificationSoundGroupIdx { get; set; } = 1;


        public bool UseSpellCheck { get; set; } = false;

        public string UserLanguage { get; set; } = "English";

        public bool ShowItemPreview { get; set; } = false;

        public bool NotificationDoPasteSound { get; set; } = true;

        public bool NotificationDoCopySound { get; set; } = true;

        public bool NotificationShowCopyToast { get; set; } = true;
        public bool NotificationDoLoadedSound { get; set; } = true;

        public string NotificationLoadedPath { get; set; } = @"Sounds/MonkeySound1.wav";

        public string NotificationCopySoundCustomPath { get; set; }
        public string NotificationAppendModeOnSoundPath { get; set; } = @"Sounds/blip2.wav";

        public string NotificationAppendModeOffSoundPath { get; set; } = @"Sounds/blip2.wav";
        public bool NotificationDoModeChangeSound { get; set; } = true;

        public bool NotificationShowModeChangeToast { get; set; } = true;

        public bool NotificationShowAppendBufferToast { get; set; } = false;
        public bool NotificationShowCopyItemTooLargeToast { get; set; }

        public bool DoShowMainWindowWithMouseEdgeAndScrollDelta { get; set; } = true;

        public bool DoShowMainWindowWithMouseEdge { get; set; } = true;
        #endregion

        #region Drag & Drop
        public string[] PasteAsImageDefaultProcessNameCollection { get; set; }

        #endregion

        #region Preferences
        public string KnownFileExtensionsPsv { get; set; } = MpRegEx.KnownFileExtensions;

        public int MaxRtfCharCount { get; set; } = 250000;

        public bool LoadOnLogin { get; set; } = false;

        public bool IgnoreWhiteSpaceCopyItems { get; set; } = true;

        public bool ResetClipboardAfterMonkeyPaste { get; set; }

        public double ThisAppDip { get; set; } = 1.0d;

        public string UserDefaultBrowserProcessPath { get; set; } = string.Empty;

        public bool DoFindBrowserUrlForCopy { get; set; } = true;

        public int MainWindowMonitorIdx { get; set; } = 0;

        public int DoShowMainWIndowWithMouseEdgeIndex { get; set; } = 1;
        #endregion

        #region Account
        public string UserName { get; set; } = "Not Set";

        public string UserEmail { get; set; } = "tkefauver@gmail.com";

        public bool IsTrialExpired { get; set; }

        public bool IsInitialLoad { get; set; } = true;
        #endregion

        #region Search Filters

        public bool SearchByIsCaseSensitive { get; set; }

        public bool SearchByWholeWord { get; set; }

        public bool SearchByContent { get; set; } = true;

        public bool SearchByUrlTitle { get; set; } = true;

        public bool SearchByApplicationName { get; set; } = true;
        public bool SearchByFileType { get; set; } = true;
        public bool SearchByImageType { get; set; } = true;
        public bool SearchByProcessName { get; set; }
        public bool SearchByTextType { get; set; } = true;
        public bool SearchBySourceUrl { get; set; } = true;
        public bool SearchByTag { get; set; }
        public bool SearchByTitle { get; set; } = true;

        public bool SearchByDescription { get; set; }

        public bool SearchByRegex { get; set; }

        public string LastQueryInfoJson { get; set; } = string.Empty;

        #endregion

        #endregion

        #region User/Device Derived Models

        [JsonIgnore]
        public MpSource ThisAppSource { get; set; }
        
        [JsonIgnore]
        public MpSource ThisOsFileManagerSource { get; set; }
        
        [JsonIgnore]
        public MpIcon ThisAppIcon { get; set; }
        
        [JsonIgnore]
        public MpUserDevice ThisUserDevice { get; set; }

        #endregion


        #region MpIJsonObject Implementation
        public string Serialize() {
            return MpJsonObject.SerializeObject(this);
        }

        #endregion

        #region Preferences Properties
        [JsonIgnore]
        public static string PreferencesPath => _prefPath;
            //Path.Combine(
            //    Environment.CurrentDirectory,
            //    PREFERENCES_FILE_NAME);

        [JsonIgnore]
        public bool IsSaving { get; private set; }

        [JsonIgnore]
        public static bool IsLoading { get; private set; } = false;
        #endregion

        #endregion

        #region Constructors

        public MpPrefViewModel() : base(){
            PropertyChanged += MpJsonPreferenceIO_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public static async Task InitAsync(string prefPath, MpIDbInfo dbInfo, MpIOsInfo osInfo) {
            _prefPath = prefPath;
            _dbInfo = dbInfo;
            _osInfo = osInfo;
            if (!File.Exists(_prefPath)) {
                await CreateDefaultPrefsAsync();
            } else {
                await LoadPrefsAsync();
            }
            
        }

        public void Save() {
            Task.Run(() => {
                //while (IsSaving) {
                //    await Task.Delay(100);
                //}

                lock (_lock) {
                    IsSaving = true;

                    var sw = Stopwatch.StartNew();

                    string prefStr = Serialize();

                    if (UseEncryption) {
                        prefStr = MpEncryption.SimpleEncryptWithPassword(prefStr, "testtesttest");
                    }

                    MpFileIo.WriteTextToFile(PreferencesPath, prefStr, false);

                    MpConsole.WriteLine("Preferences Updated Total Ms: " + sw.ElapsedMilliseconds);

                    IsSaving = false;
                }
            }).FireAndForgetSafeAsync(this);
        }
        #endregion

        #region Private Methods

        private void MpJsonPreferenceIO_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == nameof(IsSaving) || IsLoading) {
                return;
            }
            Save();
        }

        private static async Task LoadPrefsAsync() {
            IsLoading = true;

            string prefsStr;
            if (UseEncryption) {
                string prefsStr_Encrypted = MpFileIo.ReadTextFromFile(PreferencesPath);
                prefsStr = MpEncryption.SimpleDecryptWithPassword(prefsStr_Encrypted, "testtesttest");
            } else {
                prefsStr = MpFileIo.ReadTextFromFile(PreferencesPath);
            }

            MpPrefViewModel prefVm = null;
            if (ValidatePrefData(prefsStr)) {
                try {
                    prefVm = MpJsonObject.DeserializeObject<MpPrefViewModel>(prefsStr);
                }
                catch(Exception ex) {
                    MpConsole.WriteTraceLine($"Error loading pref file from '{PreferencesPath}' ", ex);
                }
            }
            
            if(prefVm == null) {
                await CreateDefaultPrefsAsync(true);
                if(Instance == null) {
                    // shouldn't happen
                    Debugger.Break();
                }
            }else {
                Instance = prefVm;
            }

            IsLoading = false;
        }

        private static bool ValidatePrefData(string prefStr) {
            
            if(string.IsNullOrWhiteSpace(prefStr)) {
                return false;
            }
            if(prefStr.StartsWith("{"+Environment.NewLine) && 
                prefStr.EndsWith(Environment.NewLine+"}")) {
                return true;
            }
            return false;
        }

        private static async Task CreateDefaultPrefsAsync(bool isReset = false) {
            MpConsole.WriteTraceLine("Pref file was either missing or empty, (re)creating");

            Instance = new MpPrefViewModel();
            if(isReset) {
                bool success = await MpDb.ResetPreferenceDefaultsAsync(_dbInfo, _osInfo);
            }
            Instance.Save();

            // NOTE this line should be removed and is only valid for current wpf db
            //Instance.ThisDeviceGuid = "f64b221e-806a-4e28-966a-f9c5ff0d9370";
            while (Instance.IsSaving) {
                await Task.Delay(100);
            }
            return;
        }

        #endregion
    }
}
