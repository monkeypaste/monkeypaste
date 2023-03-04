using System.Collections;
using System.Threading.Tasks;

namespace MonkeyPaste.Common.Plugin {
    public interface MpIAnalyzeAsyncComponent : MpIPluginComponentBase {
        Task<MpAnalyzerPluginResponseFormat> AnalyzeAsync(MpAnalyzerPluginRequestFormat req);
    }
    public interface MpIAnalyzeComponent : MpIPluginComponentBase {
        MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req);
    }

}
