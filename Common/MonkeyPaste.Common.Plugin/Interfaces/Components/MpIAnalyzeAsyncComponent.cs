using System.Threading.Tasks;

namespace MonkeyPaste.Common.Plugin {
    public interface MpIAnalyzeAsyncComponent : MpIPluginComponentBase {
        Task<MpAnalyzerPluginResponseFormat> AnalyzeAsync(MpAnalyzerPluginRequestFormat request);
    }
    public interface MpIAnalyzerComponent : MpIPluginComponentBase {
        MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat args);
    }
}
