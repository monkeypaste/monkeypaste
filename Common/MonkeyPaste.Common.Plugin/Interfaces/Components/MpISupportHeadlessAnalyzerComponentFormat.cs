namespace MonkeyPaste.Common.Plugin {
    public interface MpISupportHeadlessAnalyzerComponentFormat {
        MpAnalyzerPluginFormat GetFormat();
    }

    public interface MpISupportHeadlessClipboardComponentFormat {
        MpClipboardHandlerFormats GetFormats();
    }
}
