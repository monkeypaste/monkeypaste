using System;

namespace MonkeyPaste {
    public interface MpIStartupState {
        DateTime? LoadedDateTime { get; }
        bool IsInitialStartup { get; }
        bool IsCoreLoaded { get; }
        bool IsPlatformLoaded { get; }
    }
}
