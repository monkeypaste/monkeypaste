using MonkeyPaste.Common;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIJsImporter {
        Task ImportAllAsync();
    }
    public interface MpIDeviceWrapper {
        MpIJsImporter JsImporter { get; }
        MpIPlatformInfo PlatformInfo { get; }
        MpIPlatformScreenInfoCollection ScreenInfoCollection { get; }
    }
}
