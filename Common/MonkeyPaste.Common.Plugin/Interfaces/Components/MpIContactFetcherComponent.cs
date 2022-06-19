using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Common.Plugin {
    public interface MpIContactFetcherComponentBase : MpIPluginComponentBase { }

    public interface MpIContactFetcherComponent : MpIContactFetcherComponentBase {
        IEnumerable<MpIContact> FetchContacts(object args);
    }

    public interface MpIContactFetcherComponentAsync : MpIContactFetcherComponentBase {
        Task<IEnumerable<MpIContact>> FetchContactsAsync(object args);
    }
}
