using MonkeyPaste.Common;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIAppBuilder {
        Task<MpApp> CreateAsync(MpPortableProcessInfo handleOrAppPath);
    }
}
