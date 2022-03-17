namespace MonkeyPaste.Plugin {
    public interface MpINativeDataFormatConverter {
        string GetNativeFormatName(MpClipboardFormatType portableType);
        MpClipboardFormatType GetPortableFormatType(string nativeFormatName);
    }
}
