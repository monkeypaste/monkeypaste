namespace MonkeyPaste.Avalonia {
    public static class MpLedgerConstants {
        public static bool USE_LOCAL_LEDGER => true;

        public const string REMOTE_LEDGER_NAME = "ledger.json";
        public const string LOCAL_LEDGER_NAME = "ledger-local.json";

        public const string LEDGER_PROJ_DIR = "C:/Users/tkefauver/Source/Repos/MonkeyPaste/Common/Ledger";

        public static string DEBUG_PACKAGES_DIR =>
            $"{LEDGER_PROJ_DIR}/plugin_packages_debug/";
        public static string RELEASE_PACKAGES_DIR =>
            $"{LEDGER_PROJ_DIR}/plugin_packages_release/";

        public static string PLUGIN_PACKAGES_DIR =>
#if DEBUG
                DEBUG_PACKAGES_DIR;
#else
                RELEASE_PACKAGES_DIR;
#endif

        public static string LOCAL_LEDGER_URI =>
            $"file:///{LEDGER_PROJ_DIR}/{LOCAL_LEDGER_NAME}";
        public static string REMOTE_LEDGER_URI =>
            $"https://raw.githubusercontent.com/monkeypaste/ledger/master/{REMOTE_LEDGER_NAME}";

    }
}
