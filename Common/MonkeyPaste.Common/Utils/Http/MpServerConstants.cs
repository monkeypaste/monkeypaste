namespace MonkeyPaste.Avalonia {
    public static class MpServerConstants {

        const string LOCAL_SERVER_URL = "https://localhost";
        const string REMOTE_SERVER_URL = "https://www.monkeypaste.com";

        public const bool IS_SERVER_LOCAL = true;
        public const string DOMAIN_URL = IS_SERVER_LOCAL ? LOCAL_SERVER_URL : REMOTE_SERVER_URL;

        public static string LEGAL_BASE_URL =>
            $"{DOMAIN_URL}/legal";
        public static string ACCOUNTS_BASE_URL =>
            $"{DOMAIN_URL}/accounts";
        public static string DOCS_BASE_URL =>
            IS_SERVER_LOCAL ?
            $"{DOMAIN_URL}/docs/build/docs" :
            $"{DOMAIN_URL}/docs";

        public static string BLOG_BASE_URL =>
            IS_SERVER_LOCAL ?
            $"{DOMAIN_URL}/docs/build/blog" :
            $"{DOMAIN_URL}/blog";

    }
}
