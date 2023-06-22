using System;

namespace MonkeyPaste {
    [Flags]
    public enum MpStartupFlags {
        None = 0,
        Initial = 1,
        Login = 2,
        UserInvoked = 4
    }
}
