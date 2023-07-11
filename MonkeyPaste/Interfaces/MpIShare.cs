using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIShare {
        Task ShareTextAsync(string title, string text, object anchor = null);
        Task ShareUri(string title, string uri, object anchor = null);
    }
}
