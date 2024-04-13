using Avalonia.Platform;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvShare {

        async Task PlatformRequestAsync(MpAvShareTextRequest request) {
            await Task.Delay(1);
        }

        async Task PlatformRequestAsync(MpAvShareMultipleFilesRequest request) {
            await Task.Delay(1);

        }

    }
}
