using System;

namespace MonkeyPaste {
    public interface MpIStartupState {
        DateTime? LoadedDateTime { get; }
    }
}
