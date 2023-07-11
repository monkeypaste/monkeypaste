using System;

namespace MonkeyPaste {
    public interface MpIStartupState {
        DateTime? LoadedDateTime { get; }
        MpStartupFlags StartupFlags { get; }
        bool IsCoreLoaded { get; }
        bool IsPlatformLoaded { get; }
        bool IsReady { get; }
    }
}
