using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpILoadOnLoginTools {
        bool IsLoadOnLoginEnabled { get; }
        Task SetLoadOnLoginAsync(bool isLoadOnLogin, bool silent = false);
    }
}
