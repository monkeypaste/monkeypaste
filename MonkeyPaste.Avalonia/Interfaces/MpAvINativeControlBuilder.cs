using Avalonia.Platform;
using System;

namespace MonkeyPaste.Avalonia {
    public interface MpAvINativeControlBuilder {
        IPlatformHandle Build(IPlatformHandle parent, Func<IPlatformHandle> createDefault, object args);
    }
}
