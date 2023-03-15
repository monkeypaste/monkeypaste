using MonkeyPaste.Common;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MonkeyPaste {
    public class MpPrefViewModel :
        MpViewModelBase,
        MpICustomCsvFormat,
        MpIUserProvidedFileExts,
        MpIJsonObject {
        #region Private
        [JsonIgnore]
        private object _lock = new object();

        [JsonIgnore]
        private static string _prefPath;

        [JsonIgnore]
        private static MpIDbInfo _dbInfo;

        [JsonIgnore]
        private static MpIPlatformInfo _osInfo;
        #endregion

        #region Constants

        [JsonIgnore]
        public const string STRING_ARRAY_SPLIT_TOKEN = "<&>SPLIT</&>";

        [JsonIgnore]
        public const string PREF_BACKUP_PATH_EXT = "backup";
        #endregion

        #region Statics
        [JsonIgnore]
        static ReaderWriterLock locker = new ReaderWriterLock();

        [JsonIgnore]
        private static MpPrefViewModel _instance;
        [JsonIgnore]
        public static MpPrefViewModel Instance =>
            _instance;

        [JsonIgnore]
        public static string PreferencesPath => _prefPath;

        [JsonIgnore]
        public static string PreferencesPathBackup =>
            $"{PreferencesPath}.{PREF_BACKUP_PATH_EXT}";

        #endregion

        #region Interfaces
        #region MpIUserProvidedFileExts Implementation
        string MpIUserProvidedFileExts.UserDefineExtPsv =>
            UserDefinedFileExtensionsPsv;

        #endregion

        #region MpICustomCsvFormat Implementation
        MpCsvFormatProperties MpICustomCsvFormat.CsvFormat =>
            MpCsvFormatProperties.DefaultBase64Value;

        #endregion

        #region MpIJsonObject Implementation
        public string SerializeJsonObject() {
            return MpJsonConverter.SerializeObject(this);
        }

        #endregion

        #endregion

        #region Properties

        #region Property Reflection Referencer

        [SuppressPropertyChangedWarnings]
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

        #region User/Device Derived Models    
        public string ThisDeviceGuid { get; set; } = System.Guid.NewGuid().ToString();
        #endregion

        #region Editor

        #endregion

        #region Encyption
        public string SslAlgorithm { get; set; } = "SHA256WITHRSA";
        public string SslCASubject { get; set; } = "CN=MPCA";
        public string SslCertSubject { get; set; } = "CN=127.0.01";
        #endregion

        public string LocalStoragePath =>
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        #region Db        

        public bool EncryptDb { get; set; } = true;


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

        #region Ole

        // This is used to discern core cb handler so it is automatically enabled on first startup (not the typical workflow)
        public string CoreClipboardHandlerGuid => "cf2ec03f-9edd-45e9-a605-2a2df71e03bd";

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

        #endregion

        #region Sound

        public string NotificationCopySound1Path {
            get {
                return @"Sounds/Ting.wav";
            }
        }


        public bool NotificationDoPasteSound { get; set; } = true;

        public bool NotificationDoCopySound { get; set; } = true;

        public bool NotificationDoLoadedSound { get; set; } = true;
        public string NotificationCopySoundCustomPath { get; set; }
        public bool NotificationDoModeChangeSound { get; set; } = true;
        public string NotificationLoadedPath { get; set; } = @"Sounds/MonkeySound1.wav";
        public string NotificationAppendModeOnSoundPath { get; set; } = @"Sounds/blip2.wav";

        public string NotificationAppendModeOffSoundPath { get; set; } = @"Sounds/blip2.wav";

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

        #region Dynamic Properties          

        #region Account

        public string UserEmail { get; set; } = "tkefauver@gmail.com";

        #endregion

        #region Preferences

        #region Look & Feel

        public string CurrentThemeName { get; set; } = MpThemeType.Light.ToString();
        public int NotificationSoundGroupIdx { get; set; } = 1; // not shown
        public double NotificationSoundVolume { get; set; } = 0;
        public bool ShowInTaskbar { get; set; } = true;
        public bool ShowInTaskSwitcher { get; set; } = true;
        public bool ShowMainWindowOnDragToScreenTop { get; set; } = true;

        public string DefaultReadOnlyFontFamily { get; set; } = "Segoe UI";
        public string DefaultEditableFontFamily { get; set; } = "Arial";
        public int DefaultFontSize { get; set; } = 12;
        public double GlobalBgOpacity { get; set; }
#if DESKTOP
        = 0.7;
#else
        = 1.0d;
#endif

        #endregion

        #region Language

        public string UserLanguageCode { get; set; } = CultureInfo.CurrentCulture.Name;

        #endregion

        #region History

        public int MaxUndoLimit { get; set; } = 10;
        public int MaxRecentTextsCount { get; set; } = 10;

        public int MaxStagedClipCount { get; set; } = 25;

        public bool TrackExternalPasteHistory { get; set; } = false; // will show warning about storage or something
        #endregion

        #region System

        public bool LoadOnLogin { get; set; } = false;

        #endregion

        #region Content

        public bool IsDuplicateCheckEnabled { get; set; } = true;

        public bool IsRichHtmlContentEnabled { get; set; } = true;
        public bool IgnoreAppendedItems { get; set; } = true;
        public bool IsSpellCheckEnabled { get; set; } = true;

        public bool IgnoreInternalClipboardChanges { get; set; } = true;
        public bool IgnoreWhiteSpaceCopyItems { get; set; } = true;
        public bool ResetClipboardAfterMonkeyPaste { get; set; }

        #endregion

        public string UserDefinedFileExtensionsPsv { get; set; } = string.Empty;


        #endregion

        #region Security
        public string IgnoredProcessNames { get; set; } = string.Empty;
        public bool IsSettingsEncrypted { get; set; } = false; // requires restart and only used to trigger convert on exit (may not be necessary to restart)

        public string DbPassword { get; set; } = MpPasswordGenerator.GetRandomPassword();
        #endregion

        #region Shortcuts

        public bool DoShowMainWindowWithMouseEdgeAndScrollDelta { get; set; } = true;
        public bool DoShowMainWindowWithMouseEdge { get; set; } = true;
        public string MainWindowShowBehaviorType { get; set; } = MpMainWindowShowBehaviorType.Primary.ToString();

        #endregion

        #region Runtime/Dependant Properties

        #region Language

        public string FlowDirectionName {
            get {
                if (CultureInfo.GetCultureInfo(UserLanguageCode) is CultureInfo ci &&
                    ci.TextInfo.IsRightToLeft) {
                    return "RightToLeft";
                }
                return "LeftToRight";
            }
        }

        #endregion

        #region Auto-Complete
        public string RecentFindTexts { get; set; } = string.Empty;

        public string RecentReplaceTexts { get; set; } = string.Empty;

        public string RecentSearchTexts { get; set; } = string.Empty;
        #endregion

        #region Ignored Ntf

        public string DoNotShowAgainNotificationIdCsvStr { get; set; } = string.Empty;

        #endregion

        #region Last Load Remembers

        public string MainWindowOrientation { get; set; }
#if DESKTOP
        = MpMainWindowOrientationType.Bottom.ToString();
#else
        = MpMainWindowOrientationType.Left.ToString();
#endif
        public double MainWindowInitialWidth { get; set; } = 0;
        public double MainWindowInitialHeight { get; set; } = 0;

        public DateTime StartupDateTime { get; set; } = DateTime.MinValue;
        public DateTime? LastStartupDateTime { get; set; } = null;

        public int UniqueContentItemIdx { get; set; } = 0;

        public string ClipTrayLayoutTypeName { get; set; } = MpClipTrayLayoutType.Stack.ToString();
        #endregion

        #region Encrytion

        public string SslPrivateKey { get; set; } = string.Empty;

        public string SslPublicKey { get; set; } = string.Empty;

        public DateTime SslCertExpirationDateTime { get; set; } = DateTime.UtcNow.AddDays(-1);

        #endregion

        #region Db
        #endregion

        #region Sync
        public int SyncPort { get; set; } = 11000;
        #endregion

        #region Account

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

        public bool SearchByAnnotation { get; set; }

        public bool SearchByRegex { get; set; }

        public string LastQueryInfoJson { get; set; } = string.Empty;

        #endregion

        #endregion

        #endregion

        #region State

        [JsonIgnore]
        public bool IsSaving { get; private set; }

        [JsonIgnore]
        public static bool IsLoading { get; private set; } = false;
        #endregion

        #endregion

        #region Constructors

        public MpPrefViewModel() : base() {
            PropertyChanged += MpJsonPreferenceIO_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public static async Task InitAsync(string prefPath, MpIDbInfo dbInfo, MpIPlatformInfo osInfo) {
            _prefPath = prefPath;
            _dbInfo = dbInfo;
            _osInfo = osInfo;
            if (File.Exists(_prefPath)) {
                await LoadPrefsAsync();
            } else {
                await CreateDefaultPrefsAsync();
            }
        }

        public void Save() {
            //Mp.Services.MainThreadMarshal.RunOnMainThread(async () => {
            //Task.Run(async () => {
            //    while (IsSaving) {
            //        await Task.Delay(100);
            //    }

            //lock (_lock) {

            try {
                locker.AcquireWriterLock(int.MaxValue);
                IsSaving = true;

                var sw = Stopwatch.StartNew();

                string prefStr = SerializeJsonObject();

                if (IsSettingsEncrypted) {
                    prefStr = MpEncryption.SimpleEncryptWithPassword(prefStr, GetPrefPassword());
                }

                MpFileIo.WriteTextToFile(PreferencesPath, prefStr, false);
                // write backup after succesful save
                MpFileIo.WriteTextToFile(PreferencesPathBackup, prefStr, false);

                MpConsole.WriteLine("Preferences Updated Total Ms: " + sw.ElapsedMilliseconds);

                IsSaving = false;
            }
            finally {
                locker.ReleaseWriterLock();
            }

            //}
            // }).FireAndForgetSafeAsync(this);
        }
        #endregion

        #region Private Methods

        private void MpJsonPreferenceIO_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(IsSaving) || IsLoading) {
                return;
            }
            Save();
        }

        private static string GetPrefPassword() {
            string seed = $"{Environment.UserName}{Environment.MachineName}";
            return seed.CheckSum();
        }
        private static async Task LoadPrefsAsync() {
            IsLoading = true;

            string prefsStr = MpFileIo.ReadTextFromFile(PreferencesPath);
            if (IsEncrypted(prefsStr)) {
                prefsStr = MpEncryption.SimpleDecryptWithPassword(prefsStr, GetPrefPassword());
            }

            MpPrefViewModel prefVm = null;
            if (ValidatePrefData(prefsStr)) {
                try {
                    prefVm = MpJsonConverter.DeserializeObject<MpPrefViewModel>(prefsStr);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Error loading pref file from '{PreferencesPath}' ", ex);
                }
            }

            if (prefVm == null) {
                // this means pref file is invalid, likely app crashed while saving so attempt recovery
                await CreateDefaultPrefsAsync(true);
                if (Instance == null) {
                    // shouldn't happen
                    Debugger.Break();
                }
            } else {
                _instance = prefVm;
            }

            IsLoading = false;
        }

        private static bool ValidatePrefData(string prefStr) {
            if (string.IsNullOrWhiteSpace(prefStr)) {
                return false;
            }
            if (prefStr.StartsWith("{") &&
                prefStr.EndsWith("}")) {
                return true;
            }
            return false;
        }
        private static bool IsEncrypted(string prefStr) {
            return prefStr != null && prefStr.Length > 10 && !ValidatePrefData(prefStr);
        }

        private static async Task CreateDefaultPrefsAsync(bool isReset = false) {
            MpConsole.WriteLine("Pref file was either missing, empty or this is initial startup. (re)creating");

            if (isReset) {
                if (PreferencesPathBackup.IsFile()) {
                    string backup_str = MpFileIo.ReadTextFromFile(PreferencesPathBackup);
                    if (ValidatePrefData(backup_str)) {
                        // pref is corrupt, check it and backup etc.
                        Debugger.Break();
                        MpFileIo.WriteTextToFile(PreferencesPath, backup_str, false);
                        await InitAsync(_prefPath, _dbInfo, _osInfo);
                        return;
                    }
                }
                _instance = new MpPrefViewModel();
                var info_tuple = await MpDefaultDataModelTools.DiscoverPrefInfoAsync(_dbInfo, _osInfo);
                string discovered_device_guid = info_tuple.Item1;
                int total_count = info_tuple.Item2;
                if (string.IsNullOrEmpty(discovered_device_guid)) {
                    // this means no machine name/os type or just os type match was found in db file
                    // which would be strange and will wait to handle but should probably
                    // create a device guid...
                    Debugger.Break();
                } else {
                    IsLoading = true;
                    Instance.ThisDeviceGuid = discovered_device_guid;
                    Instance.UniqueContentItemIdx = total_count;
                }
            } else {
                _instance = new MpPrefViewModel();
            }

            IsLoading = true;

            // init last queryinfo to default values
            Instance.LastQueryInfoJson = Instance.SerializeJsonObject();

            IsLoading = false;

            Instance.Save();

            while (Instance.IsSaving) {
                await Task.Delay(100);
            }

        }

        #endregion
    }
}
