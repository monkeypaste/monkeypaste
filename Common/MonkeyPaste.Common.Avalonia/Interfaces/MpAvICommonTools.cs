using Avalonia.Input.Platform;

namespace MonkeyPaste.Common.Avalonia {
    public interface MpAvICommonTools {
        IClipboard DeviceClipboard { get; set; }
    }
}
