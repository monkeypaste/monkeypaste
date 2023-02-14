using Avalonia.Platform;
using System;

namespace MonkeyPaste.Common.Avalonia {
    public interface MpAvIPlatformControl {
        IPlatformHandle CreateControl(bool isSecond, IPlatformHandle parent, Func<IPlatformHandle> createDefault);
    }
}
