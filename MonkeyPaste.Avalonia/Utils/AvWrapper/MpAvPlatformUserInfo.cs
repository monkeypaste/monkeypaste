//using Avalonia.Win32;

using System;

namespace MonkeyPaste.Avalonia {
    public class MpAvPlatformUserInfo : MpIPlatformUserInfo {
        public string UserSid =>
            // TODO this NEEDS to be a constant associated with logged in user
            // use platform store id (or something) where available
            // as-is if username changes db cannot be accessed

            Environment.UserName;

        public string UserEmail =>
            // NOTE this may not be needed but is
            // supposed to associate local w/ server db user records
            "test@test.com";
    }
}
