namespace MonkeyPaste.Avalonia {
    public static class MpServerConstants {


        public const bool IS_SERVER_LOCAL = true;

        const string LOCAL_SERVER_URL =
#if WINDOWS
        "https://localhost";
#else
        "http://192.168.43.33";
#endif
        const string REMOTE_SERVER_URL = "https://www.monkeypaste.com";

        public const string DOMAIN_URL = IS_SERVER_LOCAL ? LOCAL_SERVER_URL : REMOTE_SERVER_URL;

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


    }

    public static class MpLedgerConstants {
        public const string REMOTE_LEDGER_NAME = "ledger.json";
        public const string LOCAL_LEDGER_NAME = "ledger-local.json";
        public const bool IS_LEDGER_LOCAL = true;

        public const string LOCAL_LEDGER_DIR = "C:/Users/tkefauver/Source/Repos/MonkeyPaste/Common/Ledger";
        public static string LOCAL_RELEASE_DIR =>
            $"{LOCAL_LEDGER_DIR}/plugin_releases/";

        public static string LEDGER_URI =>
            IS_LEDGER_LOCAL ?
            $"file:///{LOCAL_LEDGER_DIR}/ledger-local.json" :
            $"https://raw.githubusercontent.com/monkeypaste/ledger/master/ledger.json";

    }
}
