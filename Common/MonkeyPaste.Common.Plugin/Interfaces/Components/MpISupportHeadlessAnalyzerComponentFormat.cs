namespace MonkeyPaste.Common.Plugin {
    public interface MpIHeadlessComponentFormatBase { }
    public interface MpISupportHeadlessAnalyzerComponentFormat : MpIHeadlessComponentFormatBase {
        MpAnalyzerPluginFormat GetFormat();
    }

}
