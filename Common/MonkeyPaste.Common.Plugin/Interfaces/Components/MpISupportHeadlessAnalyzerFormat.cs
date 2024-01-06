namespace MonkeyPaste.Common.Plugin {

    public interface MpIHeadlessComponentFormatBase : MpIPluginComponentBase { }
    public interface MpISupportHeadlessAnalyzerFormat : MpIHeadlessComponentFormatBase {
        MpAnalyzerPluginFormat GetFormat(MpHeadlessComponentFormatRequest request);
    }
    public interface MpISupportHeadlessClipboardComponentFormat : MpIHeadlessComponentFormatBase {
        MpClipboardHandlerFormats GetFormats(MpHeadlessComponentFormatRequest request);
    }
}
