using Microsoft.Extensions.Configuration;
using MonkeyPaste.Common;
using System;

namespace MonkeyPaste.Avalonia {
    public static class MpServerConstants {
        private static IConfiguration _config;
        static IConfigurationSection Secrets {
            get {
                try {
                    if (_config == null) {
                        var cb = new ConfigurationBuilder();
                        _config = cb.AddUserSecrets(typeof(MpServerConstants).Assembly).Build();
                    }
                    return _config.GetSection("server");
                } catch {

                }
                return null;
            }
        }

        public static bool IS_SERVER_LOCAL =>
#if PUBLIC_BUILD
            true;
#else
            false;
#endif

        public static string LOCAL_SERVER_URL =>
#if WINDOWS
         "https://localhost";
#else
        Secrets == null ? "https://localhost" : Secrets["localIp"];
#endif
        public static string REMOTE_SERVER_URL =>
            Secrets == null ? 
                "https://localhost" : 
                Secrets["httpUrl"];

        public static string DOMAIN_URL =
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
            Secrets == null ? 
                "mailto:support@localhost.com" : 
                $"mailto:support@{Secrets["httpUrl"].Replace("https://www.",string.Empty)}";

    }
}
