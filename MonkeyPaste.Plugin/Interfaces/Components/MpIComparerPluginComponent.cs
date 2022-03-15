namespace MonkeyPaste.Plugin {
    public interface MpIComparerPluginComponent : MpIPluginComponentBase {
        bool CompareText(bool text);
        bool CompareImage(string imgBase64);
        bool CompareFile(string path);
    }
}
