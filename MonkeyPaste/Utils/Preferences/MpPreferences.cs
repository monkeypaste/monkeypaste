using Newtonsoft.Json.Linq;
using System;
using System.IO;
using Xamarin.Essentials;

namespace MonkeyPaste {
    public static class MpPreferences {
        #region Constructors

        public static void Init(MpIPreferenceIO prefIo) {
            Default = prefIo;
            if (string.IsNullOrEmpty(ThisDeviceGuid)) {
                ThisDeviceGuid = System.Guid.NewGuid().ToString();
            }
            ResetClipboardAfterMonkeyPaste = false;
        }

        #endregion

        #region Private Variables

        public static MpIPreferenceIO Default { get; private set; }
        #endregion

        #region Properties

        public static readonly string STRING_ARRAY_SPLIT_TOKEN = "<&>SPLIT</&>";

        #region Property Reflection Referencer

        //public static object this[string propertyName] {
        //    get {
        //        // probably faster without reflection:
        //        // like:  return Properties.Settings.Default.PropertyValues[propertyName] 
        //        // instead of the following
        //        Type myType = typeof(MpPreferences);
        //        PropertyInfo myPropInfo = myType.GetProperty(propertyName);
        //        if (myPropInfo == null) {
        //            throw new Exception("Unable to find property: " + propertyName);
        //        }
        //        return myPropInfo.GetValue(this, null);
        //    }
        //    set {
        //        Type myType = typeof(MpPreferences);
        //        PropertyInfo myPropInfo = myType.GetProperty(propertyName);
        //        myPropInfo.SetValue(this, value, null);
        //    }
        //}

        #endregion

        #region Application Properties

        #region Encyption
        public static string SslAlgorithm { get; set; } = "SHA256WITHRSA";
        public static string SslCASubject { get; set; } = "CN{ get; set; } =MPCA";
        public static string SslCertSubject { get; set; } = "CN{ get; set; } =127.0.01";
        #endregion

        public static MpUserDeviceType ThisDeviceType {
            get {
                return Default.GetDeviceType();
            }
        }

        public static string LocalStoragePath {
            get {
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
        }

        #region Db

        public static string DbName { get; set; } = "Mp.db";

        public static string DbPath {
            get {
                return Default.Get(nameof(DbPath), Path.Combine(LocalStoragePath, DbName));
            }
            set {
                Default.Set(nameof(DbPath), value);
            }
        }

        public static string DbMediaFolderPath {
            get {
                return Default.Get(nameof(DbMediaFolderPath), Path.Combine(LocalStoragePath, "media"));
            }
            set {
                Default.Set(nameof(DbMediaFolderPath), value);
            }
        }

        public static int MaxDbPasswordAttempts {
            get {
                return 3;
            }
        }
        #endregion

        #region Sync
        public static string SyncCertFolderPath {
            get {
                return Path.Combine(LocalStoragePath, "SyncCerts");
            }
        }

        public static string SyncCaPath {
            get {
                return Path.Combine(SyncCertFolderPath, @"MPCA.cert");
            }
        }

        public static string SyncCertPath {
            get {
                return Path.Combine(SyncCertFolderPath, @"MPSC.cert");
            }
        }

        public static string SyncServerProtocol {
            get {
                return Default.Get(nameof(SyncServerProtocol), @"https://");
            }
            set {
                Default.Set(nameof(SyncServerProtocol), value);
            }
        }

        public static string SyncServerHostNameOrIp {
            get {
                return Default.Get(nameof(SyncServerHostNameOrIp), @"monkeypaste.com");
            }
            set {
                Default.Set(nameof(SyncServerHostNameOrIp), value);
            }
        }

        public static int SyncServerPort {
            get {
                return Default.Get(nameof(SyncServerPort), 44376);
            }
            set {
                Default.Set(nameof(SyncServerPort), value);
            }
        }

        public static string SyncServerEndpoint {
            get {
                return $"{SyncServerProtocol}{SyncServerHostNameOrIp}:{SyncServerPort}";
            }
        }
        #endregion

        #region Appearance
        public static double LogWindowHeightRatio {
            get {
                return 0.35;
            }
        }

        public static double MainWindowStartHeight {
            get {
                return 10000;
            }
        }
        #endregion

        #region Resources

        public static string AbsoluteResourcesPath {
            get {
                return @"pack://application:,,,/Resources";
            }
        }

        public static int MaxFilePathCharCount {
            get {
                return 260;
            }
        }

        #endregion

        #region Drag & Drop
        public static string CompositeItemDragDropFormatName {
            get {
                return "CompositeItemDragDropFormat";
            }
        }

        public static string ClipTileDragDropFormatName {
            get {
                return "MpClipDragDropFormat";
            }
        }
        #endregion

        #region Experience
        public static int ShowMainWindowAnimationMilliseconds {
            get {
                return 500;
            }
        }

        public static int HideMainWindowAnimationMilliseconds {
            get {
                return 250;
            }
        }

        public static int SearchBoxTypingDelayInMilliseconds {
            get {
                return 500;
            }
        }

        public static string NotificationCopySound1Path {
            get {
                return @"Sounds/Ting.wav";
            }
        }

        public static int ShowMainWindowMouseHitZoneHeight {
            get {
                return 5;
            }
        }

        public static string DefaultCultureInfoName {
            get {
                return @"en-US";
            }
        }

        public static string SearchPlaceHolderText {
            get {
                return @"Search...";
            }
        }

        public static string ApplicationName {
            get {
                return @"Monkey Paste";
            }
        }
        #endregion

        #region REST

        public static string CurrencyConverterFreeApiKey {
            get {
                return @"897d0d9538155ebeaff7";
            }
        }

        public static string AzureCognitiveServicesKey {
            get {
                return "b455280a2c66456e926b66a1e6656ce3";
            }
        }

        public static string AzureTextAnalyticsKey {
            get {
                return "ec769ed641ac48ed86b38363e67e824b";
            }
        }

        public static string AzureTextAnalyticsEndpoint {
            get {
                return @"https://mp-azure-text-analytics-services-resource-instance.cognitiveservices.azure.com/";
            }
        }

        public static string AzureCognitiveServicesEndpoint {
            get {
                return @"https://mp-azure-cognitive-services-resource-instance.cognitiveservices.azure.com/";
            }
        }

        public static string BitlyApiToken {
            get {
                return @"f6035b9ed05ac82b42d4853c984e34a4f1ba05d8";
            }
        }

        public static string RestfulOpenAiApiKey {
            get {
                return @"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";
            }
        }

        public static string DomainFavIconEndpoint {
            get {
                return @"https://www.google.com/s2/favicons?https://www.google.com/s2/favicons?sz=64&domain_url=";
            }
        }
        #endregion

        #region Settings 
        public static string AutoSelectionElementTag {
            get {
                return "AutoSelectionElement";
            }
        }
        public static int MaxCommandLineArgumentLength {
            get {
                return 1024;
            }
        }


        public static int MaxQrCodeCharLength {
            get {
                return 4296;
            }
        }

        public static int MaxTemplateTextLength {
            get {
                return 10;
            }
        }


        #endregion

        #endregion

        #region User Properties        

        public static MpSource ThisAppSource { get; set; }

        public static MpSource ThisOsFileManagerSource { get; set; }

        public static MpIcon ThisAppIcon { get; set; }

        public static MpUserDevice ThisUserDevice { get; set; }

        public static string RecentFindTexts {
            get => Default.Get(nameof(RecentFindTexts), string.Empty);
            set => Default.Set(nameof(RecentFindTexts), value);
        }


        public static string RecentReplaceTexts {
            get => Default.Get(nameof(RecentReplaceTexts), string.Empty);
            set => Default.Set(nameof(RecentReplaceTexts), value);
        }

        public static string RecentSearchTexts {
            get => Default.Get(nameof(RecentSearchTexts), string.Empty);
            set => Default.Set(nameof(RecentSearchTexts), value);
        }

        public static int MaxRecentTextsCount {
            get => Default.Get(nameof(MaxRecentTextsCount), 8);
            set => Default.Set(nameof(MaxRecentTextsCount), value);
        }

        public static string IgnoredProcessNames {
            get {
                return Default.Get(nameof(IgnoredProcessNames), string.Empty);
            }
            set {
                Default.Set(nameof(IgnoredProcessNames), value);
            }
        }
        public static string DoNotShowAgainNotificationIdCsvStr {
            // NOTE this is stored as a property because the loader window
            // is used before bootstrapping the database occurs
            get {
                return Default.Get(nameof(DoNotShowAgainNotificationIdCsvStr), string.Empty);
            }
            set {
                Default.Set(nameof(DoNotShowAgainNotificationIdCsvStr), value);
            }
        }

        public static string AppStorageFilePath {
            get {
                return Default.Get(nameof(AppStorageFilePath), Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            }
            set {
                Default.Set(nameof(AppStorageFilePath), value);
            }
        }

        public static double MainWindowInitialHeight {
            get {
                return Default.Get(nameof(MainWindowInitialHeight), default(double));
            }
            set {
                Default.Set(nameof(MainWindowInitialHeight), value);
            }
        }

        public static DateTime StartupDateTime {
            get {
                return Default.Get(nameof(StartupDateTime), DateTime.MinValue);
            }
            set {
                Default.Set(nameof(StartupDateTime), value);
            }
        }
        public static string UserCultureInfoName {
            get {
                return Default.Get(nameof(UserCultureInfoName), DefaultCultureInfoName);
            }
            set {
                Default.Set(nameof(UserCultureInfoName), value);
            }
        }

        public static int UniqueContentItemIdx {
            get {
                return Default.Get(nameof(UniqueContentItemIdx), 0);
            }
            set {
                Default.Set(nameof(UniqueContentItemIdx), value);
            }
        }

        public static string ThisDeviceGuid {
            get {
                return Default.Get(nameof(ThisDeviceGuid), string.Empty);
            }
            set {
                Default.Set(nameof(ThisDeviceGuid), value);
            }
        }
        //
        public static bool ShowMainWindowOnDragToScreenTop {
            get {
                return Default.Get(nameof(ShowMainWindowOnDragToScreenTop), true);
            }
            set {
                Default.Set(nameof(ShowMainWindowOnDragToScreenTop), value);
            }
        }
        public static int ThisAppSourceId {
            get {
                return Default.Get(nameof(ThisAppSourceId), 0);
            }
            set {
                Default.Set(nameof(ThisAppSourceId), value);
            }
        }

        public static int ThisOsFileManagerSourceId {
            get {
                return Default.Get(nameof(ThisOsFileManagerSourceId), 0);
            }
            set {
                Default.Set(nameof(ThisOsFileManagerSourceId), value);
            }
        }

        #region Encrytion

        public static string SslPrivateKey {
            get {
                return Default.Get(nameof(SslPrivateKey), string.Empty);
            }
            set {
                Default.Set(nameof(SslPrivateKey), value);
            }
        }

        public static string SslPublicKey {
            get {
                return Default.Get(nameof(SslPublicKey), string.Empty);
            }
            set {
                Default.Set(nameof(SslPublicKey), value);
            }
        }

        public static DateTime SslCertExpirationDateTime {
            get {
                return Default.Get(nameof(SslCertExpirationDateTime), DateTime.UtcNow.AddDays(-1));
            }
            set {
                Default.Set(nameof(SslCertExpirationDateTime), value);
            }
        }

        public static string FallbackProcessPath {
            get {
                return Default.Get(nameof(FallbackProcessPath), @"C:\WINDOWS\Explorer.EXE");
            }
            set {
                Default.Set(nameof(FallbackProcessPath), value);
            }
        }
        #endregion

        #region Db

        public static bool EncryptDb {
            get {
                return Default.Get(nameof(EncryptDb), true);
            }
            set {
                Default.Set(nameof(EncryptDb), value);
            }
        }

        public static string DbPassword {
            get {
                return Default.Get(
                    nameof(DbPassword),
                    MpPasswordGenerator.GetRandomPassword());
            }
            set {
                Default.Set(nameof(DbPassword), value);
            }
        }
        #endregion

        #region Sync
        public static int SyncPort {
            get {
                return Default.Get(nameof(UserName), 11000);
            }
            set {
                Default.Set(nameof(UserName), value);
            }
        }
        #endregion

        #region REST

        public static int RestfulLinkMinificationMaxCount {
            get {
                return Default.Get(nameof(RestfulLinkMinificationMaxCount), 5);
            }
            set {
                Default.Set(nameof(RestfulLinkMinificationMaxCount), value);
            }
        }

        public static int RestfulDictionaryDefinitionMaxCount {
            get {
                return Default.Get(nameof(RestfulDictionaryDefinitionMaxCount), 5);
            }
            set {
                Default.Set(nameof(RestfulDictionaryDefinitionMaxCount), value);
            }
        }

        public static int RestfulTranslationMaxCount {
            get {
                return Default.Get(nameof(RestfulTranslationMaxCount), 5);
            }
            set {
                Default.Set(nameof(RestfulTranslationMaxCount), value);
            }
        }
        public static int RestfulCurrencyConversionMaxCount {
            get {
                return Default.Get(nameof(RestfulCurrencyConversionMaxCount), 5);
            }
            set {
                Default.Set(nameof(RestfulCurrencyConversionMaxCount), value);
            }
        }

        public static int RestfulLinkMinificationCount {
            get {
                return Default.Get(nameof(RestfulLinkMinificationCount), 0);
            }
            set {
                Default.Set(nameof(RestfulLinkMinificationCount), value);
            }
        }

        public static int RestfulDictionaryDefinitionCount {
            get {
                return Default.Get(nameof(RestfulDictionaryDefinitionCount), 0);
            }
            set {
                Default.Set(nameof(RestfulDictionaryDefinitionCount), value);
            }
        }

        public static int RestfulCurrencyConversionCount {
            get {
                return Default.Get(nameof(RestfulCurrencyConversionCount), 0);
            }
            set {
                Default.Set(nameof(RestfulCurrencyConversionCount), value);
            }
        }

        public static int RestfulTranslationCount {
            get {
                return Default.Get(nameof(RestfulTranslationCount), 0);
            }
            set {
                Default.Set(nameof(RestfulTranslationCount), value);
            }
        }

        public static DateTime RestfulBillingDate {
            get {
                return Default.Get(nameof(RestfulBillingDate), DateTime.UtcNow);
            }
            set {
                Default.Set(nameof(RestfulBillingDate), value);
            }
        }

        public static int RestfulOpenAiCount {
            get {
                return Default.Get(nameof(RestfulOpenAiCount), 0);
            }
            set {
                Default.Set(nameof(RestfulOpenAiCount), value);
            }
        }

        public static int RestfulOpenAiMaxCount {
            get {
                return Default.Get(nameof(RestfulOpenAiMaxCount), 5);
            }
            set {
                Default.Set(nameof(RestfulOpenAiMaxCount), value);
            }
        }
        #endregion

        #region Experience
        public static string UserCustomColorIdxArray {
            get {
                return Default.Get(nameof(UserCustomColorIdxArray), "0");
            }
            set {
                Default.Set(nameof(UserCustomColorIdxArray), value);
            }
        }


        public static string ThemeClipTileBackgroundColor {
            get {
                return Default.Get(nameof(ThemeClipTileBackgroundColor), "#FFFFF");
            }
            set {
                Default.Set(nameof(ThemeClipTileBackgroundColor), value);
            }
        }

        public static string HighlightFocusedHexColorString {
            get {
                return Default.Get(nameof(HighlightFocusedHexColorString), "#FFC0CB");
            }
            set {
                Default.Set(nameof(HighlightFocusedHexColorString), value);
            }
        }

        public static string HighlightColorHexString {
            get {
                return Default.Get(nameof(HighlightColorHexString), "#FFFF00");
            }
            set {
                Default.Set(nameof(HighlightColorHexString), value);
            }
        }

        public static string ClipTileBackgroundColor {
            get {
                return Default.Get(nameof(HighlightFocusedHexColorString), "#FFFFFF");
            }
            set {
                Default.Set(nameof(HighlightFocusedHexColorString), value);
            }
        }

        public static string DefaultFontFamily {
            get {
                return Default.Get(nameof(DefaultFontFamily), "Arial");
            }
            set {
                Default.Set(nameof(DefaultFontFamily), value);
            }
        }

        public static double DefaultFontSize {
            get {
                return Default.Get(nameof(DefaultFontSize), 12.0d);
            }
            set {
                Default.Set(nameof(DefaultFontSize), value);
            }
        }

        public static string SpeechSynthVoiceName {
            get {
                return Default.Get(nameof(SpeechSynthVoiceName), "Zira");
            }
            set {
                Default.Set(nameof(SpeechSynthVoiceName), value);
            }
        }

        public static bool IgnoreNewDuplicates {
            get {
                return Default.Get(nameof(IgnoreNewDuplicates), true);
            }
            set {
                Default.Set(nameof(IgnoreNewDuplicates), value);
            }
        }

        public static int MaxRecentClipItems {
            get {
                return Default.Get(nameof(MaxRecentClipItems), 25);
            }
            set {
                Default.Set(nameof(MaxRecentClipItems), value);
            }
        }

        public static int NotificationBalloonVisibilityTimeMs {
            get {
                return Default.Get(nameof(NotificationBalloonVisibilityTimeMs), 3000);
            }
            set {
                Default.Set(nameof(NotificationBalloonVisibilityTimeMs), value);
            }
        }

        public static int NotificationSoundGroupIdx {
            get {
                return Default.Get(nameof(NotificationSoundGroupIdx), 1);
            }
            set {
                Default.Set(nameof(NotificationSoundGroupIdx), value);
            }
        }


        public static bool UseSpellCheck {
            get {
                return Default.Get(nameof(UseSpellCheck), false);
            }
            set {
                Default.Set(nameof(UseSpellCheck), value);
            }
        }

        public static string UserLanguage {
            get {
                return Default.Get(nameof(UserLanguage), "English");
            }
            set {
                Default.Set(nameof(UserLanguage), value);
            }
        }

        public static bool ShowItemPreview {
            get {
                return Default.Get(nameof(ShowItemPreview), false);
            }
            set {
                Default.Set(nameof(ShowItemPreview), value);
            }
        }

        public static bool NotificationDoPasteSound {
            get {
                return Default.Get(nameof(NotificationDoPasteSound), true);
            }
            set {
                Default.Set(nameof(NotificationDoPasteSound), value);
            }
        }

        public static bool NotificationDoCopySound {
            get {
                return Default.Get(nameof(NotificationDoCopySound), true);
            }
            set {
                Default.Set(nameof(NotificationDoCopySound), value);
            }
        }

        public static bool NotificationShowCopyToast {
            get {
                return Default.Get(nameof(NotificationShowCopyToast), true);
            }
            set {
                Default.Set(nameof(NotificationShowCopyToast), value);
            }
        }
        public static bool NotificationDoLoadedSound {
            get {
                return Default.Get(nameof(NotificationDoLoadedSound), true);
            }
            set {
                Default.Set(nameof(NotificationDoLoadedSound), value);
            }
        }

        public static string NotificationLoadedPath {
            get {
                return Default.Get(nameof(NotificationLoadedPath), @"Sounds/MonkeySound1.wav");
            }
            set {
                Default.Set(nameof(NotificationLoadedPath), value);
            }
        }

        public static string NotificationCopySoundCustomPath {
            get {
                return Default.Get(nameof(NotificationCopySoundCustomPath), string.Empty);
            }
            set {
                Default.Set(nameof(NotificationCopySoundCustomPath), value);
            }
        }
        public static string NotificationAppendModeOnSoundPath {
            get {
                return Default.Get(nameof(NotificationAppendModeOnSoundPath), @"Sounds/blip2.wav");
            }
            set {
                Default.Set(nameof(NotificationAppendModeOnSoundPath), value);
            }
        }

        public static string NotificationAppendModeOffSoundPath {
            get {
                return Default.Get(nameof(NotificationAppendModeOffSoundPath), @"Sounds/blip2.wav");
            }
            set {
                Default.Set(nameof(NotificationAppendModeOffSoundPath), value);
            }
        }
        public static bool NotificationDoModeChangeSound {
            get {
                return Default.Get(nameof(NotificationDoModeChangeSound), true);
            }
            set {
                Default.Set(nameof(NotificationDoModeChangeSound), value);
            }
        }

        public static bool NotificationShowModeChangeToast {
            get {
                return Default.Get(nameof(NotificationShowModeChangeToast), true);
            }
            set {
                Default.Set(nameof(NotificationShowModeChangeToast), value);
            }
        }

        public static bool NotificationShowAppendBufferToast {
            get {
                return Default.Get(nameof(NotificationShowAppendBufferToast), false);
            }
            set {
                Default.Set(nameof(NotificationShowAppendBufferToast), value);
            }
        }
        public static bool NotificationShowCopyItemTooLargeToast {
            get {
                return Default.Get(nameof(NotificationShowCopyItemTooLargeToast), false);
            }
            set {
                Default.Set(nameof(NotificationShowCopyItemTooLargeToast), value);
            }
        }

        public static bool DoShowMainWindowWithMouseEdgeAndScrollDelta {
            get {
                return Default.Get(nameof(DoShowMainWindowWithMouseEdgeAndScrollDelta), false);
            }
            set {
                Default.Set(nameof(DoShowMainWindowWithMouseEdgeAndScrollDelta), value);
            }
        }

        public static bool DoShowMainWindowWithMouseEdge {
            get {
                return Default.Get(nameof(DoShowMainWindowWithMouseEdge), true);
            }
            set {
                Default.Set(nameof(DoShowMainWindowWithMouseEdge), value);
            }
        }
        #endregion

        #region Drag & Drop
        public static string[] PasteAsImageDefaultProcessNameCollection {
            get {
                return Default.Get(nameof(PasteAsImageDefaultProcessNameCollection), "paint\r\nphotoshop\r\n").Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            }
            set {
                Default.Set(nameof(PasteAsImageDefaultProcessNameCollection), string.Join(Environment.NewLine, value));
            }
        }
        #endregion

        #region Preferences
        public static string KnownFileExtensionsPsv {
            get {
                return Default.Get(nameof(KnownFileExtensionsPsv), MpRegEx.KnownFileExtensions);
            }
            set {
                Default.Set(nameof(KnownFileExtensionsPsv), value);
            }
        }

        public static int MaxRtfCharCount {
            get {
                return Default.Get(nameof(MaxRtfCharCount), 250000);
            }
            set {
                Default.Set(nameof(MaxRtfCharCount), value);
            }
        }

        public static bool LoadOnLogin {
            get {
                return Default.Get(nameof(LoadOnLogin), false);
            }
            set {
                Default.Set(nameof(LoadOnLogin), value);
            }
        }

        public static bool IgnoreWhiteSpaceCopyItems {
            get {
                return Default.Get(nameof(IgnoreWhiteSpaceCopyItems), false);
            }
            set {
                Default.Set(nameof(IgnoreWhiteSpaceCopyItems), value);
            }
        }

        public static bool ResetClipboardAfterMonkeyPaste {
            get {
                return Default.Get(nameof(ResetClipboardAfterMonkeyPaste), false);
            }
            set {
                Default.Set(nameof(ResetClipboardAfterMonkeyPaste), value);
            }
        }

        public static double ThisAppDip {
            get {
                return Default.Get(nameof(ThisAppDip), (double)1);
            }
            set {
                Default.Set(nameof(ThisAppDip), value);
            }
        }

        public static string UserDefaultBrowserProcessPath {
            get {
                return Default.Get(nameof(UserDefaultBrowserProcessPath), string.Empty);
            }
            set {
                Default.Set(nameof(UserDefaultBrowserProcessPath), value);
            }
        }

        public static bool DoFindBrowserUrlForCopy {
            get {
                return Default.Get(nameof(DoFindBrowserUrlForCopy), true);
            }
            set {
                Default.Set(nameof(DoFindBrowserUrlForCopy), value);
            }
        }

        public static int MainWindowMonitorIdx {
            get {
                return Default.Get(nameof(MainWindowMonitorIdx), 0);
            }
            set {
                Default.Set(nameof(MainWindowMonitorIdx), value);
            }
        }

        public static int DoShowMainWIndowWithMouseEdgeIndex {
            get {
                return Default.Get(nameof(DoShowMainWIndowWithMouseEdgeIndex), 1);
            }
            set {
                Default.Set(nameof(DoShowMainWIndowWithMouseEdgeIndex), value);
            }
        }
        #endregion

        #region Account
        public static string UserName {
            get {
                return Default.Get(nameof(UserName), "Not Set");
            }
            set {
                Default.Set(nameof(UserName), value);
            }
        }

        public static string UserEmail {
            get {
                return Default.Get(nameof(UserEmail), "tkefauver@gmail.com");
            }
            set {
                Default.Set(nameof(UserEmail), value);
            }
        }

        public static bool IsTrialExpired {
            get {
                return Default.Get(nameof(IsTrialExpired), false);
            }
            set {
                Default.Set(nameof(IsTrialExpired), value);
            }
        }

        public static bool IsInitialLoad {
            get {
                return Default.Get(nameof(IsInitialLoad), true);
            }
            set {
                Default.Set(nameof(IsInitialLoad), value);
            }
        }
        #endregion

        #region Search Filters

        public static bool SearchByIsCaseSensitive {
            get {
                return Default.Get(nameof(SearchByIsCaseSensitive), false);
            }
            set {
                Default.Set(nameof(SearchByIsCaseSensitive), value);
            }
        }

        public static bool SearchByContent {
            get {
                return Default.Get(nameof(SearchByContent), false);
            }
            set {
                Default.Set(nameof(SearchByContent), value);
            }
        }

        public static bool SearchByUrlTitle {
            get {
                return Default.Get(nameof(SearchByUrlTitle), false);
            }
            set {
                Default.Set(nameof(SearchByUrlTitle), value);
            }
        }

        public static bool SearchByApplicationName {
            get {
                return Default.Get(nameof(SearchByApplicationName), false);
            }
            set {
                Default.Set(nameof(SearchByApplicationName), value);
            }
        }
        public static bool SearchByFileType {
            get {
                return Default.Get(nameof(SearchByFileType), false);
            }
            set {
                Default.Set(nameof(SearchByFileType), value);
            }
        }
        public static bool SearchByImageType {
            get {
                return Default.Get(nameof(SearchByImageType), false);
            }
            set {
                Default.Set(nameof(SearchByImageType), value);
            }
        }
        public static bool SearchByProcessName {
            get {
                return Default.Get(nameof(SearchByProcessName), false);
            }
            set {
                Default.Set(nameof(SearchByProcessName), value);
            }
        }
        public static bool SearchByTextType {
            get {
                return Default.Get(nameof(SearchByTextType), false);
            }
            set {
                Default.Set(nameof(SearchByTextType), value);
            }
        }
        public static bool SearchBySourceUrl {
            get {
                return Default.Get(nameof(SearchBySourceUrl), false);
            }
            set {
                Default.Set(nameof(SearchBySourceUrl), value);
            }
        }
        public static bool SearchByTag {
            get {
                return Default.Get(nameof(SearchByTag), false);
            }
            set {
                Default.Set(nameof(SearchByTag), value);
            }
        }
        public static bool SearchByTitle {
            get {
                return Default.Get(nameof(SearchByTitle), false);
            }
            set {
                Default.Set(nameof(SearchByTitle), value);
            }
        }

        public static bool SearchByDescription {
            get {
                return Default.Get(nameof(SearchByDescription), false);
            }
            set {
                Default.Set(nameof(SearchByDescription), value);
            }
        }

        public static bool SearchByRegex {
            get {
                return Default.Get(nameof(SearchByRegex), false);
            }
            set {
                Default.Set(nameof(SearchByRegex), value);
            }
        }

        #endregion


        #endregion

        #endregion

        #region MpIPreferences Implementation
        public static object GetPreferenceValue(string preferenceName) {
            return typeof(MpIPreferenceIO).GetProperty(preferenceName).GetValue(Default);
        }

        public static void SetPreferenceValue(string preferenceName, object preferenceValue) {
            typeof(MpIPreferenceIO).GetProperty(preferenceName).SetValue(Default, preferenceValue);
        }
        #endregion

        #region Private Methods


        #endregion
    }
}
