using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIUrlBuilder {
        Task<MpUrl> CreateAsync(string url, string title = "");
    }
}
