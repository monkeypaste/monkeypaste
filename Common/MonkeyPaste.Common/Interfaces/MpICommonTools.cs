namespace MonkeyPaste.Common {
    public interface MpICommonTools {
        MpIDebugBreakHelper DebugBreakHelper { get; set; }
        MpIUserAgentProvider UserAgentProvider { get; set; }
        MpIGlobalInputListener GlobalInputListener { get; set; }
        MpIProcessWatcher ProcessWatcher { get; set; }
        MpIExternalPasteHandler ExternalPasteHandler { get; set; }
        MpIUserProvidedFileExts UserProvidedFileExts { get; set; }
        MpIStringTools StringTools { get; set; }
        MpIPlatformInfo PlatformInfo { get; set; }
        MpIThisAppInfo ThisAppInfo { get; set; }
        MpIPlatformMessageBox PlatformMessageBox { get; set; }

        MpIMainThreadMarshal MainThreadMarshal { get; set; }
        MpIPlatformResource PlatformResource { get; set; }

    }
}
