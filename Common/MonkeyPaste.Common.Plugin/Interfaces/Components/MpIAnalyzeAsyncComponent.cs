using System.Threading.Tasks;

namespace MonkeyPaste.Common.Plugin {
    public interface MpIAnalyzeAsyncComponent : MpIPluginComponentBase {
        Task<object> AnalyzeAsync(object args);
    }
    public interface MpIAnalyzeComponent : MpIPluginComponentBase {
        object Analyze(object args);
    }
}
