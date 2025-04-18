﻿using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {

    public class MpAvPrefViewModel :
        MpAvViewModelBase,
        MpICustomCsvFormat,
        MpIUserProvidedFileExts,
        MpIWelcomeSetupInfo,
        MpIUserDeviceInfo {
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
            //nameof(AccountUsername),
            //nameof(AccountEmail),
            //nameof(AccountType),
            //nameof(AccountBillingCycleType),
            //nameof(AccountPassword),
            nameof(DbCreateDateTime),
            nameof(IsWelcomeComplete)
        };
        #endregion

        #region Constants

        [JsonIgnore]
        public const string DEF_SYNTAX_THEME = "monokai-sublime";
        [JsonIgnore]
        public const string PREF_FILE_NAME = "mp.pref";

        [JsonIgnore]
        public const string STRING_ARRAY_SPLIT_TOKEN = "<&>SPLIT</&>";

        [JsonIgnore]
        public const string PREF_BACKUP_PATH_EXT = "backup";

        [JsonIgnore]
        public const string BASELINE_DEFAULT_READ_ONLY_FONT = "Nunito";
        public const string BASELINE_DEFAULT_READ_ONLY_FONT2 = "Tahoma";
        public const string BASELINE_DEFAULT_CONTENT_FONT = "Arial";
        public const string BASELINE_DEFAULT_CODE_FONT = "Consolas";

        [JsonIgnore]
        public const bool ENCRYPT_DB = true;

        public const double BASE_DEFAULT_FONT_SIZE = 12;

        #endregion

        #region Statics

        [JsonIgnore]
        private static MpAvPrefViewModel _instance;
        [JsonIgnore]
        public static MpAvPrefViewModel Instance =>
            _instance;

        [JsonIgnore]
        public static string PreferencesPath =>
            _prefPath;

        [JsonIgnore]
        public static string PreferencesPathBackup =>
            $"{PreferencesPath}.{PREF_BACKUP_PATH_EXT}";


        [JsonIgnore]
        public static string DEFAULT_THEME_TYPE_NAME => MpThemeType.Dark.ToString();

        [JsonIgnore]
        public static string DEFAULT_THEME_HEX_COLOR => MpSystemColors.purple;

        [JsonIgnore]
        public static string arg1 {
            get {
#if LINUX || ANDROID
                return "arg1arg1arg1arg1arg1arg1arg1arg1";
#else
                if (!PreferencesPath.IsFile()) {
                    MpFileIo.TouchFile(PreferencesPath);
                }
                return new FileInfo(PreferencesPath).CreationTimeUtc.ToTickChecksum(); 
#endif
            }
        }
        [JsonIgnore]
        public static string arg2 =>
#if LINUX || ANDROID
            "arg2arg2arg2arg2arg2arg2arg2arg2";
#else
            Instance == null ||
            !Instance.DbCreateDateTime.HasValue ?
                string.Empty :
                Instance.DbCreateDateTime.Value.ToTickChecksum();
#endif

        [JsonIgnore]
        public static string arg3 {
            get {
#if LINUX || ANDROID
                return "arg3arg3arg3arg3arg3arg3arg3arg3";
#else
                if (!PreferencesPathBackup.IsFile()) {
                    MpFileIo.TouchFile(PreferencesPathBackup);
                }
                return new FileInfo(PreferencesPathBackup).CreationTimeUtc.ToTickChecksum();
#endif
            }
        }

#endregion

        #region Interfaces

        #region MpIUserProvidedFileExts Implementation

        public string UserDefinedFileExtensionsCsv { get; set; } = string.Empty;

        #endregion

        #region MpICustomCsvFormat Implementation
        [JsonIgnore]
        public MpCsvFormatProperties CsvFormat =>
            MpCsvFormatProperties.DefaultBase64Value;

        #endregion

        #region MpIJsonObject Implementation
        public string SerializeJsonObject() {
            return MpJsonExtensions.SerializeObject(this);
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
                Type myType = typeof(MpAvPrefViewModel);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                if (myPropInfo == null) {
                    throw new Exception("Unable to find property: " + propertyName);
                }
                return myPropInfo.GetValue(this, null);
            }
            set {
                Type myType = typeof(MpAvPrefViewModel);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);
            }
        }

        #endregion

        #region Application Properties

        #region User/Device
        public string ThisDeviceGuid { get; set; } =
#if DEBUG
            System.Guid.NewGuid().ToString();
#else
            System.Guid.NewGuid().ToString();
#endif


        public int LastLoggedInUserId { get; set; } = 0;

        #endregion

        #region Editor
        #endregion



        #region Db        
        [JsonIgnore]
        public bool EncryptDb { get; set; } = true;

        public DateTime NextTrashEmptyDateTime { get; set; } = DateTime.MaxValue;

        [JsonIgnore]
        public MpTrashCleanupModeType TrashCleanupModeType =>
            TrashCleanupModeTypeStr.ToEnum<MpTrashCleanupModeType>();

        public DateTime? DbCreateDateTime { get; set; }

        #endregion

        #region Appearance

        private bool _isWindowed = false;
        public bool IsWindowed {
            get {
#if MOBILE_OR_WINDOWED
                return true;
#else                
                return _isWindowed;
#endif
            }
            set {
                if(_isWindowed != value) {
                    _isWindowed = value;
                    OnPropertyChanged(nameof(IsWindowed));
                }
            }
        }

        public int DefaultPluginIconId { get; set; } = 0;

        [JsonIgnore]
        public MpThemeType ThemeType =>
            ThemeTypeName.ToEnum<MpThemeType>();

        [JsonIgnore]
        public bool IsThemeDark =>
            ThemeType == MpThemeType.Dark;
#endregion

#endregion

        #region Dynamic Properties          

        #region Welcome Properties

        [JsonIgnore]
        public MpShortcutRoutingProfileType DefaultRoutingProfileType {
            get => DefaultRoutingProfileTypeStr.ToEnum<MpShortcutRoutingProfileType>();
            set {
                if (DefaultRoutingProfileType != value) {
                    DefaultRoutingProfileTypeStr = value.ToString();
                    OnPropertyChanged(nameof(DefaultRoutingProfileType));
                }
            }
        }

        public string DefaultRoutingProfileTypeStr { get; set; } = MpShortcutRoutingProfileType.Default.ToString();


        #endregion

        #region Account

        public DateTime LastLoginDateTimeUtc { get; set; } = DateTime.MinValue;
        public string AccountUsername { get; set; }
        public string AccountEmail { get; set; }
        public string AccountPassword { get; set; }
        [JsonIgnore]
        public string AccountPassword2 { get; set; }

        [JsonIgnore]
        public bool AccountPrivacyPolicyAccepted { get; set; }


        [JsonIgnore]
        public MpUserAccountType AccountType {
            get => AccountTypeStr.ToEnum<MpUserAccountType>();
            set {
                if (AccountType != value) {
                    AccountTypeStr = value.ToString();
                    OnPropertyChanged(nameof(AccountType));
                }
            }
        }
        public string AccountTypeStr { get; set; } = MpUserAccountType.Free.ToString();

        [JsonIgnore]
        public MpBillingCycleType AccountBillingCycleType {
            get => AccountBillingCycleTypeStr.ToEnum<MpBillingCycleType>();
            set {
                if (AccountBillingCycleType != value) {
                    AccountBillingCycleTypeStr = value.ToString();
                    OnPropertyChanged(nameof(AccountBillingCycleType));
                }
            }
        }
        public string AccountBillingCycleTypeStr { get; set; } = MpBillingCycleType.Never.ToString();

        public DateTime AccountNextPaymentDateTime { get; set; }

        public int ContentCountAtAccountDowngrade { get; set; } = 0;

        public bool HasRated { get; set; }


        #endregion

        #region Preferences

        #region Look & Feel

        public bool IsContentWrapEnabledByDefault { get; set; } = true;
        public string SelectedSyntaxTheme { get; set; } = DEF_SYNTAX_THEME;
        public bool ShowContentTitles { get; set; } = true;
        public string ThemeTypeName { get; set; } = DEFAULT_THEME_TYPE_NAME;
        public string ThemeColor { get; set; } = DEFAULT_THEME_HEX_COLOR;
        public int NotificationSoundGroupIdx { get; set; } = (int)MpSoundGroupType.Minimal;
        public double NotificationSoundVolume { get; set; } = 0;
        public bool ShowInTaskbar { get; set; } = true;

        public bool AnimateMainWindow { get; set; } =

#if LINUX
            false;
#else
        true; 
#endif

        public string DefaultReadOnlyFontFamily { get; set; } = BASELINE_DEFAULT_READ_ONLY_FONT;
        public string DefaultEditableFontFamily { get; set; } = BASELINE_DEFAULT_CONTENT_FONT;
        public string DefaultCodeFontFamily { get; set; } = BASELINE_DEFAULT_CODE_FONT;
        public double DefaultFontSize { get; set; } = BASE_DEFAULT_FONT_SIZE;

        public bool HideCapWarnings { get; set; }
        public bool ShowHints { get; set; } = true;
        public bool ShowTooltips { get; set; } = true;
        public double GlobalBgOpacity { get; set; }
#if MULTI_WINDOW
        = 0.7;
#else
        = 1.0d;
#endif

        #endregion

        #region Language

        private string _currentCultureCode;
        public string CurrentCultureCode {
            get {
                if (string.IsNullOrWhiteSpace(_currentCultureCode)) {
                    return CultureInfo.InstalledUICulture.Name;
                }
                return _currentCultureCode;
            }
            set {
                if (_currentCultureCode != value) {
                    _currentCultureCode = value;
                    OnPropertyChanged(nameof(CurrentCultureCode));
                }
            }
        }
        public bool IsTextRightToLeft { get; set; }

        #endregion

        #region History

        public int MaxUndoLimit { get; set; } = 10;
        public int MaxRecentTextsCount { get; set; } = 10;

        public int MaxPinClipCount { get; set; } = 25;

        public bool TrackExternalPasteHistory { get; set; } = false; // will show warning about storage or something

        public string TrashCleanupModeTypeStr { get; set; } = MpTrashCleanupModeType.Never.ToString();

        public string LastSelectedSettingsTabTypeStr { get; set; }
        #endregion

        #region System

        public bool IsLoggingEnabled { get; set; }

        public bool LoadOnLogin { get; set; } = false;
        public string LastLoadedVersion { get; set; } = MpPlatformHelpers.GetAppVersion().ToString();

        #endregion

        #region Content

        public bool IsDuplicateCheckEnabled { get; set; } = true;


        private bool _isRichHtmlContentEnabled =
#if ANDROID
            false; 
#else
            true;
#endif
        public bool IsRichHtmlContentEnabled {
            get =>
#if CEFNET_WV
                !MpAvCefNetApplication.IsCefNetLoaded ? false : _isRichHtmlContentEnabled;
#else
                _isRichHtmlContentEnabled;
#endif
            set {
                if (_isRichHtmlContentEnabled != value) {

                    _isRichHtmlContentEnabled = value;
                    OnPropertyChanged(nameof(IsRichHtmlContentEnabled));
                }
            }
        }
        public bool IgnoreAppendedItems { get; set; } = true;
        public bool IsSpellCheckEnabled { get; set; } = true;
        public bool IsDataTransferDestinationFormattingEnabled { get; set; } = true;

        public bool IgnoreInternalClipboardChanges { get; set; } = true;
        public bool IgnoreWhiteSpaceCopyItems { get; set; } = true;
        public bool ResetClipboardAfterMonkeyPaste { get; set; }

        public bool AddClipboardOnStartup { get; set; } = false;
        public bool IsClipboardListeningOnStartup { get; set; } = true;

        #endregion

        #endregion

        #region Security
        public bool IsSettingsEncrypted { get; set; } =
#if ANDROID
            false;
#else
            true;
#endif
        // requires restart and only used to trigger convert on exit (may not be necessary to restart)

        public string RememberedDbPassword { get; set; }

        #endregion

        #region Shortcuts
        public bool IsDropWidgetEnabled { get; set; } = false;
        [JsonIgnore]
        public MpScrollToOpenAndLockType ScrollToOpenAndLockType {
            get => ScrollToOpenAndLockTypeStr.ToEnum<MpScrollToOpenAndLockType>();
            set {
                if (ScrollToOpenAndLockType != value) {
                    ScrollToOpenAndLockTypeStr = value.ToString();
                    OnPropertyChanged(nameof(ScrollToOpenAndLockType));
                }
            }
        }
        public string ScrollToOpenAndLockTypeStr { get; set; } = MpScrollToOpenAndLockType.None.ToString();
        public bool DragToOpen { get; set; } = true;
        public bool ScrollToOpen { get; set; } = true;
        public string MainWindowShowBehaviorTypeStr { get; set; } = MpMainWindowShowBehaviorType.Primary.ToString();

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


        public string PluginDirsToUnloadCsvStr { get; set; } = string.Empty;
        public string DoNotShowAgainNotificationIdCsvStr { get; set; } = string.Empty;


        #region Last Load Remembers

        public string MainWindowOrientationStr { get; set; }
#if MULTI_WINDOW || BROWSER
        = MpMainWindowOrientationType.Bottom.ToString();
#else
        = MpMainWindowOrientationType.Left.ToString();
#endif
        public double MainWindowInitialWidth { get; set; } = 0;
        public double MainWindowInitialHeight { get; set; } = 0;

        public DateTime StartupDateTime { get; set; } = DateTime.MinValue;
        public bool IsWelcomeComplete { get; set; }

        public string ClipTrayLayoutTypeName { get; set; } = MpClipTrayLayoutType.Stack.ToString();

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

        public MpAvPrefViewModel() : base() {
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

            WriteToDisk(SerializeJsonObject(), IsSettingsEncrypted);

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

        public void RestoreDefaults() {
            // create dummy pref with default values
            // then set each non-omitted pref individually so the change flows through its intended channels
            MpAvPrefViewModel def_pref = new();
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

        private static async Task LoadPrefsAsync() {
            IsLoading = true;

            _ = ReadRawData(false, out string prefsStr);

            MpAvPrefViewModel prefVm = null;
            if (ValidatePrefData(prefsStr)) {
                try {
                    prefVm = MpJsonExtensions.DeserializeObject<MpAvPrefViewModel>(prefsStr);
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
                    raw_str = MpEncryption.SimpleDecryptWithPassword(raw_str, from_backup ? arg3 : arg1);
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
                prefStr = MpEncryption.SimpleEncryptWithPassword(prefStr, arg1);
                backupStr = MpEncryption.SimpleEncryptWithPassword(backupStr, arg3);
            }

            MpFileIo.WriteTextToFile(PreferencesPath, prefStr);
            // write backup after succesful save
            MpFileIo.WriteTextToFile(PreferencesPathBackup, backupStr);

            //MpConsole.WriteLine("Preferences Updated Total Ms: " + sw.ElapsedMilliseconds);
        }
        private static async Task CreateDefaultPrefsAsync(bool isReset = false) {
            MpConsole.WriteLine("Pref file was either missing, empty or this is initial startup. (re)creating");

            if (isReset) {
                if (PreferencesPathBackup.IsFile()) {
                    bool encrypt = ReadRawData(true, out string backup_str);

                    if (ValidatePrefData(backup_str)) {
                        // pref is corrupt, check it and backup etc.
                        //MpDebug.Break($"Pref corrupt or missing but backup ok");
                        WriteToDisk(backup_str, encrypt);
                        await InitAsync(_prefPath, _dbInfo, _osInfo);
                        return;
                    }
                }
                _instance = new MpAvPrefViewModel();
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
                _ = arg1;
                await Task.Delay(500 + MpRandom.Rand.Next(1000));
                _ = arg3;
                _instance = new MpAvPrefViewModel();
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

        #endregion
    }
}
