using System.Threading.Tasks;

namespace MonkeyPaste.Common.Plugin {
    public interface MpISupportDeferredValueAsync {
        Task<MpPluginDeferredParameterValueResponseFormat> RequestParameterValueAsync(MpPluginDeferredParameterValueRequestFormat req);
    }

    public interface MpISupportDeferredValue {
        MpPluginDeferredParameterValueResponseFormat RequestParameterValue(MpPluginDeferredParameterValueRequestFormat req);
    }
}
