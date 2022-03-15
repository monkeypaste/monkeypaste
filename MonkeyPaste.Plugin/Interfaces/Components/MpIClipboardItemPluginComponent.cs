namespace MonkeyPaste.Plugin {
    public interface MpIClipboardItemPluginComponent : MpIPluginComponentBase {
        object GetDataObject();
        string[] GetHandledDataFormats();
    }
}
