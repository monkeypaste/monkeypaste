using System;

namespace MonkeyPaste {
    [Flags]
    public enum MpStartupFlags {
        None = 0,
        Login = 1,
        UserInvoked = 4
    }
}
