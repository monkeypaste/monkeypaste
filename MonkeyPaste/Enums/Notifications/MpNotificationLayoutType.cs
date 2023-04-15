namespace MonkeyPaste {
    public enum MpNotificationLayoutType {
        //Default = 0,
        Message,
        // Append,
        Loader,
        Warning, //confirm
        UserAction, //retry/ignore/quit
        Error, //confirm
        ErrorWithOption, //retry/ignore/quit
        ErrorAndShutdown //confirm
    }
}
