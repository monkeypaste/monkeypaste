namespace MonkeyPaste.Plugin {
    public interface MpINativeDataFormatConverter {
        string GetNativeFormatName(MpClipboardFormatType portableType, string fallbackName = "");
    }
}
