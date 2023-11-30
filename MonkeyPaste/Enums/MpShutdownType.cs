//using Avalonia.Win32;

namespace MonkeyPaste {
    public enum MpShutdownType {
        None = 0,
        TopLevelException,
        EditorResourceUpdate,
        ResourceUpdate,
        TermsDeclined,
        MainWindowClosed,
        DbAuthFailed,
        FrameworkExit,
        UserNtfCmd,
        UserTrayCmd,
        Restart
    }
}
