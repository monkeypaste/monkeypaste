using Microsoft.Extensions.Configuration;
using MonkeyPaste.Common;
using System;

namespace MonkeyPaste.Avalonia {
    public static class MpServerConstants {
        public static bool IS_SERVER_LOCAL =>
#if PUBLIC_BUILD
            true;
#else
            false;
#endif

        static string LOCAL_DOMAIN =>
#if WINDOWS
         "localhost";
#else
        // local network server (windows xampp)
        "192.168.43.33";
#endif
        static string REMOTE_DOMAIN =>
            "monkeypaste.com";
        static string DOMAIN =>
            IS_SERVER_LOCAL ?
                LOCAL_DOMAIN :
                REMOTE_DOMAIN;
        public static string LOCAL_SERVER_URL =>
            $"https://{LOCAL_DOMAIN}";
        public static string REMOTE_SERVER_URL =>
            $"https://www.{REMOTE_DOMAIN}";

        public static string DOMAIN_URL =>
            $"https://{DOMAIN}";

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
            $"mailto:support@{DOMAIN}";
    }
}
