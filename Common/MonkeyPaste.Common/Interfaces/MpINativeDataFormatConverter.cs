namespace MonkeyPaste.Common {
    public interface MpINativeDataFormatConverter {
        string GetNativeFormatName(MpPortableDataFormat portableType, string fallbackName = "");
    }
}
