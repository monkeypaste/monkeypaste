using MonkeyPaste.Common;
using System.IO;

namespace MonkeyPaste.Avalonia {
    public static class MpLedgerConstants {
        public static bool USE_LOCAL_LEDGER => false;

        public const string CULTURE_DIR_NAME = "Cultures";
        public const string LEDGER_PREFIX = "ledger";
        public const string LEDGER_EXT = "json";
        public const string LOCAL_SUFFIX = "-local";
        public const string INDEX_SUFFIX = "-index";


        public const string REMOTE_LEDGER_PREFIX = $"{LEDGER_PREFIX}";
        public const string LOCAL_LEDGER_PREFIX = $"{LEDGER_PREFIX}{LOCAL_SUFFIX}";

        public const string REMOTE_LEDGER_NAME = $"{REMOTE_LEDGER_PREFIX}.{LEDGER_EXT}";
        public const string LOCAL_LEDGER_NAME = $"{LOCAL_LEDGER_PREFIX}.{LEDGER_EXT}";

        public const string REMOTE_LEDGER_INDEX_NAME = $"{LEDGER_PREFIX}{INDEX_SUFFIX}.{LEDGER_EXT}";
        public const string LOCAL_LEDGER_INDEX_NAME = $"{LEDGER_PREFIX}{LOCAL_SUFFIX}{INDEX_SUFFIX}.{LEDGER_EXT}";

        public static string REMOTE_CORE_PLUGIN_BASE_URI => $"{MpServerConstants.REMOTE_SERVER_URL}/dat";

        public static string LEDGER_PROJ_DIR =>
            Path.Combine(
                MpPlatformHelpers.GetSolutionDir(),
                "Build",
                "Ledger");
        public static string DEBUG_PACKAGES_DIR =>
            Path.Combine(
                LEDGER_PROJ_DIR, 
                "plugin_packages_debug");
        public static string RELEASE_PACKAGES_DIR =>
            Path.Combine(
                LEDGER_PROJ_DIR,
                "plugin_packages_release");

        public static string LOCAL_INV_LEDGER_PATH =>
            Path.Combine(
                LEDGER_PROJ_DIR, 
                LOCAL_LEDGER_NAME);
        public static string REMOTE_INV_LEDGER_PATH =>
            Path.Combine(
                LEDGER_PROJ_DIR,
                REMOTE_LEDGER_NAME);
        public static string LOCAL_CULTURES_DIR_URI =>
            $"{LEDGER_PROJ_DIR.ToFileSystemUriFromPath()}/{CULTURE_DIR_NAME}";
        public static string REMOTE_CULTURES_DIR_URI =>
            $"https://raw.githubusercontent.com/monkeypaste/ledger/master/{CULTURE_DIR_NAME}";

        public static string LOCAL_LEDGER_INDEX_URI =>
            $"{LEDGER_PROJ_DIR.ToFileSystemUriFromPath()}/{LOCAL_LEDGER_INDEX_NAME}";
        public static string REMOTE_LEDGER_INDEX_URI =>
            $"https://raw.githubusercontent.com/monkeypaste/ledger/master/{REMOTE_LEDGER_INDEX_NAME}";


        public static string LOCAL_INV_LEDGER_URI =>
            $"{LEDGER_PROJ_DIR.ToFileSystemUriFromPath()}/{LOCAL_LEDGER_NAME}";
        public static string REMOTE_INV_LEDGER_URI =>
            $"https://raw.githubusercontent.com/monkeypaste/ledger/master/{REMOTE_LEDGER_NAME}";


    }
}
