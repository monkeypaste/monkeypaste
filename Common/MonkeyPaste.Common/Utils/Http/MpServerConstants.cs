namespace MonkeyPaste.Avalonia {
    public static class MpServerConstants {

        const string LOCAL_SERVER_URL = "https://localhost";
        const string REMOTE_SERVER_URL = "https://www.monkeypaste.com";

        public const bool IS_SERVER_LOCAL = true;
        public const string DOMAIN_URL = IS_SERVER_LOCAL ? LOCAL_SERVER_URL : REMOTE_SERVER_URL;

    }
}
