using System.Threading.Tasks;
using MonkeyPaste.Common;

namespace MonkeyPaste {
    public interface MpIAppBuilder {
        Task<MpApp> CreateAsync(MpPortableProcessInfo handleOrAppPath);
    }
}
