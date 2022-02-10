
using System.Threading.Tasks;

namespace MonkeyPaste.Plugin {
    public interface MpIPlugin { 
    }

    public interface MpIClipboardItemPluginComponent : MpIPlugin {
        object GetDataObject();
        string[] GetHandledDataFormats();
    }

    public interface MpIAnalyzerPluginComponent : MpIPlugin {
        Task<object> AnalyzeAsync(object args);
    }

    public interface MpITransformerPluginComponent : MpIPlugin {
        string TransformText(string text);
        string TransformImage(string imgBase64);
        string TransformFile(string path);
    }

    public interface MpIComparerPluginComponent : MpIPlugin {
        bool CompareText(bool text);
        bool CompareImage(string imgBase64);
        bool CompareFile(string path);
    }
}
