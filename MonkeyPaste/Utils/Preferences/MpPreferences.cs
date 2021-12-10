using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpPreferences {
        #region Singleton
        private static readonly Lazy<MpPreferences> _Lazy = new Lazy<MpPreferences>(() => new MpPreferences());
        public static MpPreferences Instance { get { return _Lazy.Value; } }

        private MpPreferences() {
        }

        public void Init(MpIPreferenceIO prefIo) {
            _prefIo = prefIo;

            if (string.IsNullOrEmpty(ThisDeviceGuid)) {
                MpPreferences.Instance.ThisDeviceGuid = System.Guid.NewGuid().ToString();
            }
        }
        #endregion

        #region Private Variables

        private MpIPreferenceIO _prefIo;
        #endregion

        #region Properties

        #region Application Properties

        #region Encyption
        public string SslAlgorithm { get; set; } = "SHA256WITHRSA";
        public string SslCASubject { get; set; } = "CN{ get; set; } =MPCA";
        public string SslCertSubject { get; set; } = "CN{ get; set; } =127.0.01";
        #endregion

        public MpUserDeviceType ThisDeviceType {
            get {
                return _prefIo.GetDeviceType();
            }
        }

        public string LocalStoragePath {
            get {
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
        }

        #region Db

        public string DbName { get; set; } = "Mp.db";
        public int MinDbPasswordLength { get; set; } = 12;
        public int MaxDbPasswordLength { get; set; } = 18;

        public string DbPath {
            get {
                return _prefIo.Get(nameof(DbPath), Path.Combine(LocalStoragePath, DbName));
            }
        }

        public string DbMediaFolderPath {
            get {
                return _prefIo.Get(nameof(DbMediaFolderPath), Path.Combine(LocalStoragePath, "media"));
            }
            set {
                _prefIo.Set(nameof(DbMediaFolderPath), value);
            }
        }

        public int MaxDbPasswordAttempts {
            get {
                return 3;
            }
        }
        #endregion

        #region Sync
        public string SyncCertFolderPath {
            get {
                return Path.Combine(LocalStoragePath, "SyncCerts");
            }
        }

        public string SyncCaPath {
            get {
                return Path.Combine(SyncCertFolderPath, @"MPCA.cert");
            }
        }

        public string SyncCertPath {
            get {
                return Path.Combine(SyncCertFolderPath, @"MPSC.cert");
            }
        }

        public string SyncServerProtocol {
            get {
                return _prefIo.Get(nameof(SyncServerProtocol), @"https://");
            }
            set {
                _prefIo.Set(nameof(SyncServerProtocol), value);
            }
        }

        public string SyncServerHostNameOrIp {
            get {
                return _prefIo.Get(nameof(SyncServerHostNameOrIp), @"monkeypaste.com");
            }
            set {
                _prefIo.Set(nameof(SyncServerHostNameOrIp), value);
            }
        }

        public int SyncServerPort {
            get {
                return _prefIo.Get(nameof(SyncServerPort), 44376);
            }
            set {
                _prefIo.Set(nameof(SyncServerPort), value);
            }
        }

        public string SyncServerEndpoint {
            get {
                return $"{SyncServerProtocol}{SyncServerHostNameOrIp}:{SyncServerPort}";
            }
        }
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

        public MpSource ThisAppSource { get; set; }

        public MpUserDevice ThisUserDevice { get; set; }

        public double MainWindowInitialHeight {
            get {
                return _prefIo.Get(nameof(MainWindowInitialHeight), default(double));
            }
            set {
                _prefIo.Set(nameof(MainWindowInitialHeight), value);
            }
        }

        public bool SearchByDescription {
            get {
                return _prefIo.Get(nameof(SearchByDescription), false);
            }
            set {
                _prefIo.Set(nameof(SearchByDescription), value);
            }
        }

        public DateTime StartupDateTime {
            get {
                return _prefIo.Get(nameof(StartupDateTime), DateTime.MinValue);
            }
            set {
                _prefIo.Set(nameof(StartupDateTime), value);
            }
        }
        public string UserCultureInfoName {
            get {
                return _prefIo.Get(nameof(UserCultureInfoName), DefaultCultureInfoName);
            }
            set {
                _prefIo.Set(nameof(UserCultureInfoName), value);
            }
        }

        public string ThisDeviceGuid {
            get {
                return _prefIo.Get(nameof(ThisDeviceGuid), string.Empty);
            }
            set {
                _prefIo.Set(nameof(ThisDeviceGuid), value);
            }
        }

        public int ThisDeviceSourceId {
            get {
                return _prefIo.Get(nameof(ThisDeviceSourceId), 0);
            }
            set {
                _prefIo.Set(nameof(ThisDeviceSourceId), value);
            }
        }

        #region Encrytion

        public string SslPrivateKey {
            get {
                return _prefIo.Get(nameof(SslPrivateKey), string.Empty);
            }
            set {
                _prefIo.Set(nameof(SslPrivateKey), value);
            }
        }

        public string SslPublicKey {
            get {
                return _prefIo.Get(nameof(SslPublicKey), string.Empty);
            }
            set {
                _prefIo.Set(nameof(SslPublicKey), value);
            }
        }

        public DateTime SslCertExpirationDateTime {
            get {
                return _prefIo.Get(nameof(SslCertExpirationDateTime), DateTime.UtcNow.AddDays(-1));
            }
            set {
                _prefIo.Set(nameof(SslCertExpirationDateTime), value);
            }
        }
        #endregion

        #region Db

        public bool EncryptDb {
            get {
                return _prefIo.Get(nameof(EncryptDb), true);
            }
            set {
                _prefIo.Set(nameof(EncryptDb), value);
            }
        }

        public string DbPassword {
            get {
                return _prefIo.Get(
                    nameof(DbPassword), 
                    MpHelpers.Instance.GetRandomString(
                        MpHelpers.Instance.Rand.Next(
                            MinDbPasswordLength, 
                            MaxDbPasswordLength), 
                        MpHelpers.Instance.PasswordChars));
            }
        }
        #endregion

        #region Sync
        public int SyncPort {
            get {
                return _prefIo.Get(nameof(UserName), 11000);
            }
            set {
                _prefIo.Set(nameof(UserName), value);
            }
        }
        #endregion

        #region REST

        public int RestfulLinkMinificationMaxCount {
            get {
                return _prefIo.Get(nameof(RestfulLinkMinificationMaxCount), 5);
            }
            set {
                _prefIo.Set(nameof(RestfulLinkMinificationMaxCount), value);
            }
        }

        public int RestfulDictionaryDefinitionMaxCount {
            get {
                return _prefIo.Get(nameof(RestfulDictionaryDefinitionMaxCount), 5);
            }
            set {
                _prefIo.Set(nameof(RestfulDictionaryDefinitionMaxCount), value);
            }
        }

        public int RestfulTranslationMaxCount {
            get {
                return _prefIo.Get(nameof(RestfulTranslationMaxCount), 5);
            }
            set {
                _prefIo.Set(nameof(RestfulTranslationMaxCount), value);
            }
        }
        public int RestfulCurrencyConversionMaxCount {
            get {
                return _prefIo.Get(nameof(RestfulCurrencyConversionMaxCount), 5);
            }
            set {
                _prefIo.Set(nameof(RestfulCurrencyConversionMaxCount), value);
            }
        }

        public int RestfulLinkMinificationCount {
            get {
                return _prefIo.Get(nameof(RestfulLinkMinificationCount), 0);
            }
            set {
                _prefIo.Set(nameof(RestfulLinkMinificationCount), value);
            }
        }

        public int RestfulDictionaryDefinitionCount {
            get {
                return _prefIo.Get(nameof(RestfulDictionaryDefinitionCount), 0);
            }
            set {
                _prefIo.Set(nameof(RestfulDictionaryDefinitionCount), value);
            }
        }

        public int RestfulCurrencyConversionCount {
            get {
                return _prefIo.Get(nameof(RestfulCurrencyConversionCount), 0);
            }
            set {
                _prefIo.Set(nameof(RestfulCurrencyConversionCount), value);
            }
        }

        public int RestfulTranslationCount {
            get {
                return _prefIo.Get(nameof(RestfulTranslationCount), 0);
            }
            set {
                _prefIo.Set(nameof(RestfulTranslationCount), value);
            }
        }

        public DateTime RestfulBillingDate {
            get {
                return _prefIo.Get(nameof(RestfulBillingDate), DateTime.UtcNow);
            }
            set {
                _prefIo.Set(nameof(RestfulBillingDate), value);
            }
        }

        public int RestfulOpenAiCount {
            get {
                return _prefIo.Get(nameof(RestfulOpenAiCount), 0);
            }
            set {
                _prefIo.Set(nameof(RestfulOpenAiCount), value);
            }
        }

        public int RestfulOpenAiMaxCount {
            get {
                return _prefIo.Get(nameof(RestfulOpenAiMaxCount), 5);
            }
            set {
                _prefIo.Set(nameof(RestfulOpenAiMaxCount), value);
            }
        }
        #endregion

        #region Experience
        public System.Int32[] UserCustomColorIdxArray {
            get {
                return _prefIo.Get(nameof(UserCustomColorIdxArray), new Int32[] { 0 });
            }
            set {
                _prefIo.Set(nameof(UserCustomColorIdxArray), value);
            }
        }

        

        public string ThemeClipTileBackgroundColor {
            get {
                return _prefIo.Get(nameof(ThemeClipTileBackgroundColor), "#FFFFF");
            }
            set {
                _prefIo.Set(nameof(ThemeClipTileBackgroundColor), value);
            }
        }

        public string HighlightFocusedHexColorString {
            get {
                return _prefIo.Get(nameof(HighlightFocusedHexColorString), "#FFC0CB");
            }
            set {
                _prefIo.Set(nameof(HighlightFocusedHexColorString), value);
            }
        }

        public string HighlightColorHexString {
            get {
                return _prefIo.Get(nameof(HighlightColorHexString), "#FFFF00");
            }
            set {
                _prefIo.Set(nameof(HighlightColorHexString), value);
            }
        }

        public string ClipTileBackgroundColor {
            get {
                return _prefIo.Get(nameof(HighlightFocusedHexColorString), "#FFFFFF");
            }
            set {
                _prefIo.Set(nameof(HighlightFocusedHexColorString), value);
            }
        }

        public string DefaultFontFamily {
            get {
                return _prefIo.Get(nameof(DefaultFontFamily), "Arial");
            }
            set {
                _prefIo.Set(nameof(DefaultFontFamily), value);
            }
        }

        public double DefaultFontSize {
            get {
                return _prefIo.Get(nameof(DefaultFontSize), 12);
            }
            set {
                _prefIo.Set(nameof(DefaultFontSize), value);
            }
        }

        public string SpeechSynthVoiceName {
            get {
                return _prefIo.Get(nameof(SpeechSynthVoiceName), "Zira");
            }
            set {
                _prefIo.Set(nameof(SpeechSynthVoiceName), value);
            }
        }

        public bool IgnoreNewDuplicates {
            get {
                return _prefIo.Get(nameof(IgnoreNewDuplicates), true);
            }
            set {
                _prefIo.Set(nameof(IgnoreNewDuplicates), value);
            }
        }

        public int MaxRecentClipItems {
            get {
                return _prefIo.Get(nameof(MaxRecentClipItems), 25);
            }
            set {
                _prefIo.Set(nameof(MaxRecentClipItems), value);
            }
        }

        public int NotificationBalloonVisibilityTimeMs {
            get {
                return _prefIo.Get(nameof(NotificationBalloonVisibilityTimeMs), 3000);
            }
            set {
                _prefIo.Set(nameof(NotificationBalloonVisibilityTimeMs), value);
            }
        }

        public int NotificationSoundGroupIdx {
            get {
                return _prefIo.Get(nameof(NotificationSoundGroupIdx), 1);
            }
            set {
                _prefIo.Set(nameof(NotificationSoundGroupIdx), value);
            }
        }


        public bool UseSpellCheck {
            get {
                return _prefIo.Get(nameof(UseSpellCheck), false);
            }
            set {
                _prefIo.Set(nameof(UseSpellCheck), value);
            }
        }

        public string UserLanguage {
            get {
                return _prefIo.Get(nameof(UserLanguage), "English");
            }
            set {
                _prefIo.Set(nameof(UserLanguage), value);
            }
        }

        public bool ShowItemPreview {
            get {
                return _prefIo.Get(nameof(ShowItemPreview), false);
            }
            set {
                _prefIo.Set(nameof(ShowItemPreview), value);
            }
        }

        public bool NotificationDoPasteSound {
            get {
                return _prefIo.Get(nameof(NotificationDoPasteSound), true);
            }
            set {
                _prefIo.Set(nameof(NotificationDoPasteSound), value);
            }
        }

        public bool NotificationDoCopySound {
            get {
                return _prefIo.Get(nameof(NotificationDoCopySound), true);
            }
            set {
                _prefIo.Set(nameof(NotificationDoCopySound), value);
            }
        }

        public bool NotificationShowCopyToast {
            get {
                return _prefIo.Get(nameof(NotificationShowCopyToast), true);
            }
            set {
                _prefIo.Set(nameof(NotificationShowCopyToast), value);
            }
        }
        public bool NotificationDoLoadedSound {
            get {
                return _prefIo.Get(nameof(NotificationDoLoadedSound), true);
            }
            set {
                _prefIo.Set(nameof(NotificationDoLoadedSound), value);
            }
        }

        public string NotificationLoadedPath {
            get {
                return _prefIo.Get(nameof(NotificationLoadedPath), @"Sounds/MonkeySound1.wav");
            }
            set {
                _prefIo.Set(nameof(NotificationLoadedPath), value);
            }
        }

        public string NotificationCopySoundCustomPath {
            get {
                return _prefIo.Get(nameof(NotificationCopySoundCustomPath), string.Empty);
            }
            set {
                _prefIo.Set(nameof(NotificationCopySoundCustomPath), value);
            }
        }

        public string NotificationAppendModeOffSoundPath {
            get {
                return _prefIo.Get(nameof(NotificationAppendModeOffSoundPath), @"Sounds/blip2.wav");
            }
            set {
                _prefIo.Set(nameof(NotificationAppendModeOffSoundPath), value);
            }
        }
        public bool NotificationDoModeChangeSound {
            get {
                return _prefIo.Get(nameof(NotificationDoModeChangeSound), true);
            }
            set {
                _prefIo.Set(nameof(NotificationDoModeChangeSound), value);
            }
        }

        public bool NotificationShowModeChangeToast {
            get {
                return _prefIo.Get(nameof(NotificationShowModeChangeToast), true);
            }
            set {
                _prefIo.Set(nameof(NotificationShowModeChangeToast), value);
            }
        }

        public bool NotificationShowAppendBufferToast {
            get {
                return _prefIo.Get(nameof(NotificationShowAppendBufferToast), false);
            }
            set {
                _prefIo.Set(nameof(NotificationShowAppendBufferToast), value);
            }
        }
        public bool NotificationShowCopyItemTooLargeToast {
            get {
                return _prefIo.Get(nameof(NotificationShowCopyItemTooLargeToast), false);
            }
            set {
                _prefIo.Set(nameof(NotificationShowCopyItemTooLargeToast), value);
            }
        }

        public bool DoShowMainWindowWithMouseEdgeAndScrollDelta {
            get {
                return _prefIo.Get(nameof(DoShowMainWindowWithMouseEdgeAndScrollDelta), false);
            }
            set {
                _prefIo.Set(nameof(DoShowMainWindowWithMouseEdgeAndScrollDelta), value);
            }
        }

        public bool DoShowMainWindowWithMouseEdge {
            get {
                return _prefIo.Get(nameof(DoShowMainWindowWithMouseEdge), true);
            }
            set {
                _prefIo.Set(nameof(DoShowMainWindowWithMouseEdge), value);
            }
        }
        #endregion

        #region Drag & Drop
        public string[] PasteAsImageDefaultProcessNameCollection {
            get {
                return _prefIo.Get(nameof(PasteAsImageDefaultProcessNameCollection), "paint\r\nphotoshop\r\n").Split(new string[] { Environment.NewLine },StringSplitOptions.RemoveEmptyEntries);
            }
            set {
                _prefIo.Set(nameof(PasteAsImageDefaultProcessNameCollection), string.Join(Environment.NewLine,value));
            }
        }
        #endregion

        #region Preferences
        public string KnownFileExtensionsPsv {
            get {
                return _prefIo.Get(nameof(KnownFileExtensionsPsv), @"rtf|txt|jpg|jpeg|png|svg|zip|csv|gif|pdf|doc|docx|xls|xlsx");
            }
            set {
                _prefIo.Set(nameof(KnownFileExtensionsPsv), value);
            }
        }

        public int MaxRtfCharCount {
            get {
                return _prefIo.Get(nameof(MaxRtfCharCount), 250000);
            }
            set {
                _prefIo.Set(nameof(MaxRtfCharCount), value);
            }
        }

        public bool LoadOnLogin {
            get {
                return _prefIo.Get(nameof(LoadOnLogin), false);
            }
            set {
                _prefIo.Set(nameof(LoadOnLogin), value);
            }
        }

        public bool SearchByApplicationName {
            get {
                return _prefIo.Get(nameof(SearchByApplicationName), true);
            }
            set {
                _prefIo.Set(nameof(SearchByApplicationName), value);
            }
        }

        public bool SearchByTag {
            get {
                return _prefIo.Get(nameof(SearchByTag), true);
            }
            set {
                _prefIo.Set(nameof(SearchByTag), value);
            }
        }

        public bool SearchByRichText {
            get {
                return _prefIo.Get(nameof(SearchByRichText), true);
            }
            set {
                _prefIo.Set(nameof(SearchByRichText), value);
            }
        }

        public bool SearchByFileList {
            get {
                return _prefIo.Get(nameof(SearchByFileList), true);
            }
            set {
                _prefIo.Set(nameof(SearchByFileList), value);
            }
        }

        public bool SearchByTitle {
            get {
                return _prefIo.Get(nameof(SearchByTitle), true);
            }
            set {
                _prefIo.Set(nameof(SearchByTitle), value);
            }
        }

        public bool SearchByImage {
            get {
                return _prefIo.Get(nameof(SearchByImage), true);
            }
            set {
                _prefIo.Set(nameof(SearchByImage), value);
            }
        }

        public bool SearchBySourceUrl {
            get {
                return _prefIo.Get(nameof(SearchBySourceUrl), true);
            }
            set {
                _prefIo.Set(nameof(SearchBySourceUrl), value);
            }
        }

        public bool SearchByProcessName {
            get {
                return _prefIo.Get(nameof(SearchByProcessName), true);
            }
            set {
                _prefIo.Set(nameof(SearchByProcessName), value);
            }
        }

        public bool IgnoreWhiteSpaceCopyItems {
            get {
                return _prefIo.Get(nameof(IgnoreWhiteSpaceCopyItems), false);
            }
            set {
                _prefIo.Set(nameof(IgnoreWhiteSpaceCopyItems), value);
            }
        }

        public bool ResetClipboardAfterMonkeyPaste {
            get {
                return _prefIo.Get(nameof(ResetClipboardAfterMonkeyPaste), true);
            }
            set {
                _prefIo.Set(nameof(ResetClipboardAfterMonkeyPaste), value);
            }
        }

        public double ThisAppDip {
            get {
                return _prefIo.Get(nameof(ThisAppDip), (double)1);
            }
            set {
                _prefIo.Set(nameof(ThisAppDip), value);
            }
        }

        public string UserDefaultBrowserProcessPath {
            get {
                return _prefIo.Get(nameof(UserDefaultBrowserProcessPath), string.Empty);
            }
            set {
                _prefIo.Set(nameof(UserDefaultBrowserProcessPath), value);
            }
        }

        public bool DoFindBrowserUrlForCopy {
            get {
                return _prefIo.Get(nameof(DoFindBrowserUrlForCopy), true);
            }
            set {
                _prefIo.Set(nameof(DoFindBrowserUrlForCopy), value);
            }
        }

        public int MainWindowMonitorIdx {
            get {
                return _prefIo.Get(nameof(MainWindowMonitorIdx), 0);
            }
            set {
                _prefIo.Set(nameof(MainWindowMonitorIdx), value);
            }
        }

        public int DoShowMainWIndowWithMouseEdgeIndex {
            get {
                return _prefIo.Get(nameof(DoShowMainWIndowWithMouseEdgeIndex), 1);
            }
            set {
                _prefIo.Set(nameof(DoShowMainWIndowWithMouseEdgeIndex), value);
            }
        }
        #endregion

        #region Account
        public string UserName {
            get {
                return _prefIo.Get(nameof(UserName), "Not Set");
            }
            set {
                _prefIo.Set(nameof(UserName), value);
            }
        }

        public string UserEmail {
            get {
                return _prefIo.Get(nameof(UserEmail), "tkefauver@gmail.com");
            }
            set {
                _prefIo.Set(nameof(UserEmail), value);
            }
        }

        public bool IsTrialExpired {
            get {
                return _prefIo.Get(nameof(IsTrialExpired), false);
            }
            set {
                _prefIo.Set(nameof(IsTrialExpired), value);
            }
        }

        public bool IsInitialLoad {
            get {
                return _prefIo.Get(nameof(IsInitialLoad), true);
            }
            set {
                _prefIo.Set(nameof(IsInitialLoad), value);
            }
        }
        #endregion

        public bool IsSearchCaseSensitive {
            get {
                return _prefIo.Get(nameof(IsSearchCaseSensitive), false);
            }
            set {
                _prefIo.Set(nameof(IsSearchCaseSensitive), value);
            }
        }

        

        
        #endregion


        #endregion

        #region MpIPreferences Implementation
        public object GetPreferenceValue(string preferenceName) {
            return this.GetType().GetProperty(preferenceName).GetValue(this);
        }

        public void SetPreferenceValue(string preferenceName, object preferenceValue) {
            this.GetType().GetProperty(preferenceName).SetValue(this, preferenceValue);
        }
        #endregion
    }
}
