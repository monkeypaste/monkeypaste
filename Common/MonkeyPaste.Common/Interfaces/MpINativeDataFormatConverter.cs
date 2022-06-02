namespace MonkeyPaste.Plugin {
    public interface MpINativeDataFormatConverter {
        string GetNativeFormatName(MpPortableDataFormat portableType, string fallbackName = "");
    }
}
