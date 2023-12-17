namespace MonkeyPaste.Common {
    public enum MpNotificationLayoutType {
        Welcome,
        Loader,
        Message,
        Warning, //confirm
        UserAction, //retry/ignore/quit
        ErrorWithOption, //retry/ignore/quit
        ErrorAndShutdown //confirm
    }
}
