namespace MonkeyPaste.Common {
    public enum MpUserDeviceType {
        None = 0,
        Ios,
        Android,
        Windows,
        Wsl, // only using this because its in db and is a gray area but not sure how to actually detect yet
        Mac,
        Linux,
        Browser,
        Unknown
    }
}
