using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIUrlBuilder {
        Task<MpUrl> CreateAsync(string url, int appId = 0, bool suppressWrite = false);
    }
}
