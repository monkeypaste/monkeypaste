using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Common.Plugin {
    public interface MpIContactFetcherComponentBase : MpIPluginComponentBase { }

    public interface MpIContactFetcherComponent : MpIContactFetcherComponentBase {
        IEnumerable<MpIContact> Fetch(object args);
    }

    public interface MpIContactFetcherComponentAsync : MpIContactFetcherComponentBase {
        Task<IEnumerable<MpIContact>> FetchAsync(object args);
    }
}
