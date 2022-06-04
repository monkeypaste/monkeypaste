namespace MonkeyPaste.Common.Plugin {
    public interface MpITransformerPluginComponent : MpIPluginComponentBase {
        string TransformText(string text);
        string TransformImage(string imgBase64);
        string TransformFile(string path);
    }
}
