namespace MonkeyPaste.Common {
    public interface MpICommonTools {
        MpIProcessWatcher ProcessWatcher { get; set; }
        MpIExternalPasteHandler ExternalPasteHandler { get; set; }
        MpIUserProvidedFileExts UserProvidedFileExts { get; set; }
        MpIStringTools StringTools { get; set; }
        MpIPlatformInfo PlatformInfo { get; set; }
        MpINativeMessageBox NativeMessageBox { get; set; }
    }
}
