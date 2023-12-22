using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Common.Plugin {
    public interface MpIContactFetcherComponentBase : MpIPluginComponentBase { }

    public interface MpIContactFetcherComponent : MpIContactFetcherComponentBase {
        MpPluginContactFetchResponseFormat Fetch(MpPluginContactFetchRequestFormat req);
    }

    public interface MpIContactFetcherComponentAsync : MpIContactFetcherComponentBase {
        Task<MpPluginContactFetchResponseFormat> FetchAsync(MpPluginContactFetchRequestFormat req);
    }
}
