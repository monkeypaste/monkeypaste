using MonkeyPaste.Plugin;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIPluginItemViewModel<T> where T:MpViewModelBase {
        Task InitializeAsync(MpPluginFormat analyzerPlugin);
    }
}
