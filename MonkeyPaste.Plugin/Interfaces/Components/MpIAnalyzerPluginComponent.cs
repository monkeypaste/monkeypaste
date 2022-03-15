using System.Threading.Tasks;

namespace MonkeyPaste.Plugin {
    public interface MpIAnalyzerPluginComponent : MpIPluginComponentBase {
        Task<object> AnalyzeAsync(object args);
    }
}
