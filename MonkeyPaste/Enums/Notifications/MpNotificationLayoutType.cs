namespace MonkeyPaste {
    public enum MpNotificationLayoutType {
        Welcome,
        Loader,
        Message,
        Warning, //confirm
        UserAction, //retry/ignore/quit
        Error, //confirm
        ErrorWithOption, //retry/ignore/quit
        ErrorAndShutdown //confirm
    }
}
