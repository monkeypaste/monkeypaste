using MonkeyPaste.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace MonkeyPaste {
    public enum MpTrashCleanupModeType {
        Never,
        Daily,
        Weekly,
        Monthly
    }

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

        [JsonIgnore]
        private string[] _OmittedResetPropertyNames = new string[] {
            nameof(ThisDeviceGuid),
            nameof(DefaultPluginIconId),
            nameof(UserEmail),
            nameof(DbCreateDateTime),
            nameof(LastStartupDateTime),
            nameof(SslPrivateKey),
            nameof(SslPublicKey),
            nameof(SslCertExpirationDateTime),
            nameof(SyncPort),
            nameof(IsTrialExpired),
            nameof(IsInitialLoad),
        };
        #endregion

        #region Constants

        [JsonIgnore]
        public const string PREF_FILE_NAME = "mp.pref";

        [JsonIgnore]
        public const string STRING_ARRAY_SPLIT_TOKEN = "<&>SPLIT</&>";

        [JsonIgnore]
        public const string PREF_BACKUP_PATH_EXT = "backup";

        [JsonIgnore]
        public const bool ENCRYPT_DB = true;

        #endregion

        #region Statics

        [JsonIgnore]
        private static MpPrefViewModel _instance;
        [JsonIgnore]
        public static MpPrefViewModel Instance =>
            _instance;

        [JsonIgnore]
        public static string PreferencesPath =>
            _prefPath;

        [JsonIgnore]
        public static string PreferencesPathBackup =>
            $"{PreferencesPath}.{PREF_BACKUP_PATH_EXT}";

        #endregion

        #region Interfaces

        #region MpIUserProvidedFileExts Implementation

        public string UserDefinedFileExtensionsCsv { get; set; } = string.Empty;

        #endregion

        #region MpICustomCsvFormat Implementation
        [JsonIgnore]
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

        #region User/Device
        public string ThisDeviceGuid { get; set; } = System.Guid.NewGuid().ToString();

        public int LastLoggedInUserId { get; set; } = 0;
        #endregion

        #region Editor
        #endregion

        #region Encyption
        [JsonIgnore]
        public string SslAlgorithm { get; set; } = "SHA256WITHRSA";
        [JsonIgnore]
        public string SslCASubject { get; set; } = "CN=MPCA";
        [JsonIgnore]
        public string SslCertSubject { get; set; } = "CN=127.0.01";
        #endregion

        [JsonIgnore]
        public string LocalStoragePath =>
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        #region Db        
        [JsonIgnore]
        public bool EncryptDb { get; set; } = true;

        public DateTime NextTrashEmptyDateTime { get; set; } = DateTime.MaxValue;

        [JsonIgnore]
        public MpTrashCleanupModeType TrashCleanupModeType =>
            TrashCleanupModeTypeStr.ToEnum<MpTrashCleanupModeType>();

        public DateTime DbCreateDateTime { get; set; }
        #region Sync
        [JsonIgnore]
        public string SyncCertFolderPath => Path.Combine(LocalStoragePath, "SyncCerts");
        [JsonIgnore]
        public string SyncCaPath => Path.Combine(SyncCertFolderPath, @"MPCA.cert");
        [JsonIgnore]
        public string SyncCertPath => Path.Combine(SyncCertFolderPath, @"MPSC.cert");
        [JsonIgnore]
        public string SyncServerProtocol => @"https://";
        [JsonIgnore]
        public string SyncServerHostNameOrIp => "monkeypaste.com";
        [JsonIgnore]
        public int SyncServerPort { get; set; } = 44376;
        [JsonIgnore]
        public string SyncServerEndpoint => $"{SyncServerProtocol}{SyncServerHostNameOrIp}:{SyncServerPort}";

        #endregion

        #endregion

        #region Appearance

        public int DefaultPluginIconId { get; set; } = 0;

        [JsonIgnore]
        public MpThemeType ThemeType =>
            ThemeTypeName.ToEnum<MpThemeType>();

        #endregion

        #region Ole

        // This is used to discern core cb handler so it is automatically enabled on first startup (not the typical workflow)

        [JsonIgnore]
        public string CoreClipboardHandlerGuid => "cf2ec03f-9edd-45e9-a605-2a2df71e03bd";


        [JsonIgnore]
        public string CoreAnnotatorDefaultPresetGuid => "a9fa2fbf-025d-4ced-a23b-234085b5ac5f";

        #endregion

        #region Experience

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

        public string ApplicationName =>
            "MonkeyPaste";
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

        #region Welcome Properties

        // NOTE this intended for reset shortcuts/all and will be set during installer
        //public string ShortcutProfileTypeName { get; set; } = MpShortcutRoutingProfileType.Internal.ToString();
        //[JsonIgnore]
        //public MpShortcutRoutingProfileType ShortcutProfileType =>
        //    ShortcutProfileTypeName.ToEnum<MpShortcutRoutingProfileType>();

        //[JsonConverter(typeof(StringEnumConverter))]
        //public MpShortcutRoutingProfileType ShortcutProfileType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MpShortcutRoutingProfileType InitialStartupRoutingProfileType { get; set; } = MpShortcutRoutingProfileType.Internal;

        #endregion

        #region Account

        public string UserEmail { get; set; } = "tkefauver@gmail.com";

        #endregion

        #region Preferences

        #region Look & Feel

        public string ThemeTypeName { get; set; } = MpThemeType.Dark.ToString();
        public string ThemeColor { get; set; } = MpSystemColors.purple;
        public int NotificationSoundGroupIdx { get; set; } = (int)MpSoundGroupType.Minimal;
        public bool IsSoundEnabled { get; set; } = false;
        public double NotificationSoundVolume { get; set; } = 0;
        public bool ShowInTaskbar { get; set; } = true;
        public bool ShowInTaskSwitcher { get; set; } = true;

        public bool AnimateMainWindow { get; set; } = true;

        public string DefaultReadOnlyFontFamily { get; set; } = "Segoe UI";
        public string DefaultEditableFontFamily { get; set; } = "Arial";
        public int DefaultFontSize { get; set; } = 12;

        public bool ShowHints { get; set; } = true;
        public double GlobalBgOpacity { get; set; }
#if DESKTOP
        = 0.7;
#else
        = 1.0d;
#endif

        #endregion

        #region Language

        public string UserLanguageCode { get; set; } = CultureInfo.CurrentCulture.Name;
        public bool IsTextRightToLeft { get; set; } = CultureInfo.GetCultureInfo(CultureInfo.CurrentCulture.Name).TextInfo.IsRightToLeft;

        #endregion

        #region History

        public int MaxUndoLimit { get; set; } = 10;
        public int MaxRecentTextsCount { get; set; } = 10;

        public int MaxPinClipCount { get; set; } = 25;

        public bool TrackExternalPasteHistory { get; set; } = false; // will show warning about storage or something

        public string TrashCleanupModeTypeStr { get; set; } = MpTrashCleanupModeType.Never.ToString();

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

        public bool AddClipboardOnStartup { get; set; } = true;
        public bool IsClipboardListeningOnStartup { get; set; } = true;

        #endregion

        #endregion

        #region Security
        public bool IsSettingsEncrypted { get; set; } = true; // requires restart and only used to trigger convert on exit (may not be necessary to restart)

        //public string DbPassword { get; set; } = ENCRYPT_DB ? MpPasswordGenerator.GetRandomPassword() : null;
        #endregion

        #region Shortcuts
        public bool ShowExternalDropWidget { get; set; } = false;

        public bool ShowMainWindowOnDragToScreenTop { get; set; } = true;
        public bool DoShowMainWindowWithMouseEdgeAndScrollDelta { get; set; } = true;
        public string MainWindowShowBehaviorType { get; set; } = MpMainWindowShowBehaviorType.Primary.ToString();

        public bool IsAutoSearchEnabled { get; set; } = true;
        #endregion

        #region Runtime/Dependant Properties

        #region Language



        #endregion

        #region Auto-Complete

        public string RecentSearchTexts { get; set; } = string.Empty;

        public string RecentPluginSearchTexts { get; set; } = string.Empty;
        public string RecentSettingsSearchTexts { get; set; } = string.Empty;
        #endregion

        #region Ignored Ntf

        public string DoNotShowAgainNotificationIdCsvStr { get; set; } = string.Empty;

        #endregion

        #region Last Load Remembers

        public string MainWindowOrientation { get; set; }
#if DESKTOP
        = MpMainWindowOrientationType.Bottom.ToString();
#elif BROWSER
        = MpMainWindowOrientationType.Bottom.ToString();
#else
        = MpMainWindowOrientationType.Left.ToString();
#endif
        public double MainWindowInitialWidth { get; set; } = 0;
        public double MainWindowInitialHeight { get; set; } = 0;

        public DateTime StartupDateTime { get; set; } = DateTime.MinValue;
        public DateTime? LastStartupDateTime { get; set; } = null;

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
        public bool SearchByTitle { get; set; } = true;

        public bool SearchByAnnotation { get; set; }

        public bool SearchByRegex { get; set; }

        //public string LastQueryInfoJson { get; set; } = string.Empty;

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
            IsSaving = true;

            WriteToDisk(SerializeJsonObject());

            IsSaving = false;
        }


        public async Task<IList<string>> AddOrUpdateAutoCompleteTextAsync(string ac_property_name, string new_text) {
            MpDebug.Assert(this.HasProperty(ac_property_name), $"Update auto-complete error, cannot find pref property '{ac_property_name}'");
            List<string> ac_items = (this.GetPropertyValue(ac_property_name) as string).ToListFromCsv(MpCsvFormatProperties.DefaultBase64Value);
            if (string.IsNullOrEmpty(new_text)) {
                return ac_items;
            }
            while (IsSaving) {
                await Task.Delay(100);
            }
            int st_idx = ac_items.IndexOf(new_text);
            if (st_idx < 0) {
                ac_items.Insert(0, new_text);
            } else {
                ac_items.Move(st_idx, 0);
            }
            ac_items = ac_items.Take(MaxRecentTextsCount).ToList();
            this.SetPropertyValue(ac_property_name, ac_items.ToCsv(MpCsvFormatProperties.DefaultBase64Value));
            return ac_items;
        }

        #endregion

        #region Private Methods

        private void MpJsonPreferenceIO_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(IsSaving) || IsLoading) {
                return;
            }
            Save();
        }

        private bool IsPropertyResetOmitted(string propName) {
            if (_OmittedResetPropertyNames.Contains(propName)) {
                return true;
            }
            if (this.GetType().GetProperty(propName) is PropertyInfo pi) {
                return pi.GetCustomAttribute(typeof(JsonIgnoreAttribute)) != null;
            }
            return false;
        }

        private static string GetPrefPassword() {
            if (!PreferencesPath.IsFile()) {
                using (File.Create(PreferencesPath)) { }
            }
            return new FileInfo(PreferencesPath).CreationTimeUtc.ToString();
        }

        private static string GetBackupPrefPassword() {
            if (!PreferencesPathBackup.IsFile()) {
                using (File.Create(PreferencesPathBackup)) { }
            }
            return new FileInfo(PreferencesPathBackup).CreationTimeUtc.ToString();
        }
        private static async Task LoadPrefsAsync() {
            IsLoading = true;

            _ = ReadRawData(false, out string prefsStr);

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
                    MpDebug.Break();
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

        private static bool ReadRawData(bool from_backup, out string raw_str) {
            raw_str = MpFileIo.ReadTextFromFile(from_backup ? PreferencesPathBackup : PreferencesPath);
            if (IsEncrypted(raw_str)) {
                try {
                    raw_str = MpEncryption.SimpleDecryptWithPassword(raw_str, from_backup ? GetBackupPrefPassword() : GetPrefPassword());
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Error decrypting pref file '{PreferencesPath}'", ex);
                    raw_str = null;
                }
                return true;
            }
            return false;
        }
        private static void WriteToDisk(string prefStr, bool encrypt = true) {
            var sw = Stopwatch.StartNew();

            string backupStr = prefStr;
            if (encrypt) {
                prefStr = MpEncryption.SimpleEncryptWithPassword(prefStr, GetPrefPassword());
                backupStr = MpEncryption.SimpleEncryptWithPassword(backupStr, GetBackupPrefPassword());
            }

            MpFileIo.WriteTextToFile(PreferencesPath, prefStr, false);
            // write backup after succesful save
            MpFileIo.WriteTextToFile(PreferencesPathBackup, backupStr, false);

            MpConsole.WriteLine("Preferences Updated Total Ms: " + sw.ElapsedMilliseconds);
        }
        private static async Task CreateDefaultPrefsAsync(bool isReset = false) {
            MpConsole.WriteLine("Pref file was either missing, empty or this is initial startup. (re)creating");

            if (isReset) {
                if (PreferencesPathBackup.IsFile()) {
                    bool encrypt = ReadRawData(true, out string backup_str);

                    if (ValidatePrefData(backup_str)) {
                        // pref is corrupt, check it and backup etc.
                        MpDebug.Break($"Pref corrupt or missing but backup ok");
                        //MpFileIo.WriteTextToFile(PreferencesPath, backup_str, false);
                        WriteToDisk(backup_str, encrypt);
                        await InitAsync(_prefPath, _dbInfo, _osInfo);
                        return;
                    }
                }
                _instance = new MpPrefViewModel();
                string discovered_device_guid = await MpDefaultDataModelTools.DiscoverPrefInfoAsync(_dbInfo, _osInfo);
                if (string.IsNullOrEmpty(discovered_device_guid)) {
                    // this means no machine name/os type or just os type match was found in db file
                    // which would be strange and will wait to handle but should probably
                    // create a device guid...
                    MpDebug.Break();
                } else {
                    IsLoading = true;
                    Instance.ThisDeviceGuid = discovered_device_guid;
                }
            } else {
                _instance = new MpPrefViewModel();
            }

            IsLoading = true;

            // init last queryinfo to default values
            //Instance.LastQueryInfoJson = Instance.SerializeJsonObject();

            IsLoading = false;

            Instance.Save();

            while (Instance.IsSaving) {
                await Task.Delay(100);
            }

        }

        #endregion

        #region Commands

        public ICommand RestoreDefaultsCommand => new MpCommand(
            () => {
                // create dummy pref with default values
                // then set each non-omitted pref individually so the change flows through its intended channels
                MpPrefViewModel def_pref = new();
                var propNames =
                    this.GetType().GetProperties()
                    .Where(x => x.SetMethod != null && !IsPropertyResetOmitted(x.Name))
                    .Select(x => x.Name);
                MpConsole.WriteLine("Reseting prefs...", true);
                foreach (var pn in propNames) {
                    try {
                        object old_val = this.GetPropertyValue(pn);
                        this.SetPropertyValue(pn, def_pref.GetPropertyValue(pn));
                        object new_val = this.GetPropertyValue(pn);
                        MpConsole.WriteLine($"Property '{pn}' changed from '{old_val}' to '{new_val}'");
                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine($"Error reseting '{pn}'.", ex);
                    }
                }
            });
#if DEBUG
        public ICommand LogDecryptedPrefsCommand => new MpCommand(
            () => {
                MpConsole.WriteLine($"Decrypted prefs at path '{PreferencesPath}':");
                MpConsole.WriteLine(SerializeJsonObject());
            });
#endif

        #endregion
    }
}
