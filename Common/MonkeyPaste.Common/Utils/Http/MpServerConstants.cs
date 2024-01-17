namespace MonkeyPaste.Avalonia {
    public static class MpServerConstants {
        public const bool IS_SERVER_LOCAL = false;

        public const string LOCAL_SERVER_URL =
#if WINDOWS
        "https://localhost";
#else
        "http://192.168.43.33";
#endif
        public const string REMOTE_SERVER_URL = "https://www.monkeypaste.com";

        public const string DOMAIN_URL =
            IS_SERVER_LOCAL ?
                LOCAL_SERVER_URL :
                REMOTE_SERVER_URL;

        public static string LEGAL_BASE_URL =>
            $"{DOMAIN_URL}/legal";
        public static string ACCOUNTS_BASE_URL =>
            $"{DOMAIN_URL}/accounts";
        public static string PLUGINS_BASE_URL =>
            $"{DOMAIN_URL}/plugins";
        public static string VERSION_BASE_URL =>
            $"{DOMAIN_URL}/version";
        public static string DOCS_BASE_URL =>
            IS_SERVER_LOCAL ?
            $"{DOMAIN_URL}/docs/build/docs" :
            $"{DOMAIN_URL}/docs";

        public static string BLOG_BASE_URL =>
            IS_SERVER_LOCAL ?
            $"{DOMAIN_URL}/docs/build/blog" :
            $"{DOMAIN_URL}/blog";

        public static string SUPPORT_EMAIL_URI =>
            "mailto:support@monkeypaste.com";

    }
}
