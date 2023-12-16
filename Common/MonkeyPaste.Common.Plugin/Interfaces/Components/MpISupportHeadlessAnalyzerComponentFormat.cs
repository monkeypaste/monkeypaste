namespace MonkeyPaste.Common.Plugin {
    public interface MpIHeadlessComponentFormatBase : MpIPluginComponentBase { }
    public interface MpISupportHeadlessAnalyzerComponentFormat : MpIHeadlessComponentFormatBase {
        MpAnalyzerPluginFormat GetFormat();
    }

}
