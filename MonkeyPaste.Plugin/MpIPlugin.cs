
namespace MonkeyPaste.Plugin {
    public interface MpIPlugin {
        string GetName();

        MpIPluginComponent[] GetComponents();
    }

    public interface MpIPluginComponent {
        void Create(object obj);
    }

    public interface MpIClipboardItemPluginComponent : MpIPluginComponent {
        object GetDataObject();
        string[] GetHandledDataFormats();
    }

    public interface MpIAnalyzerPluginComponent : MpIPluginComponent {
        string AnalyzeText(string text);
        string AnalyzeImage(string imgBase64);
        string AnalyzeFile(string path);
    }

    public interface MpITransformerPluginComponent : MpIPluginComponent {
        string TransformText(string text);
        string TransformImage(string imgBase64);
        string TransformFile(string path);
    }

    public interface MpIComparerPluginComponent : MpIPluginComponent {
        bool CompareText(bool text);
        bool CompareImage(string imgBase64);
        bool CompareFile(string path);
    }
}
