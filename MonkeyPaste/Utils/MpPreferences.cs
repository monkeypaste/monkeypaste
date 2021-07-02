using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Essentials;

namespace MonkeyPaste {
    public class MpPreferences {
        #region Singleton
        private static readonly Lazy<MpPreferences> _Lazy = new Lazy<MpPreferences>(() => new MpPreferences());
        public static MpPreferences Instance { get { return _Lazy.Value; } }

        private MpPreferences() {

        }
        #endregion

        #region Properties

        #region Application Properties
        public const string SslAlgorithm = "SHA256WITHRSA";
        public const string SslCASubject = "CN=MPCA";
        public const string SslCertSubject = "CN=127.0.01";

        public const string DbName = "Mp.db";
        public const int MinDbPasswordLength = 12;
        public const int MaxDbPasswordLength = 18;

        public const SQLite.SQLiteOpenFlags DbFlags =
            // open the database in read/write mode
            SQLite.SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLite.SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLite.SQLiteOpenFlags.SharedCache;


        public string LocalStoragePath {
            get {
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
        }

        public string DbPath {
            get {
                return Preferences.Get(nameof(DbPath), Path.Combine(LocalStoragePath, DbName));
            }
        }

        public string DbMediaFolderPath {
            get {
                return Preferences.Get(nameof(DbMediaFolderPath), Path.Combine(LocalStoragePath, "media"));
            }
            set {
                Preferences.Set(nameof(DbMediaFolderPath), value);
            }
        }

        public string SyncCertFolderPath {
            get {
                return Path.Combine(LocalStoragePath, "SyncCerts");
            }
        }

        public string SyncCaPath {
            get {
                return Path.Combine(SyncCertFolderPath, @"MPCA.cert");
            }
        }

        public string SyncCertPath {
            get {
                return Path.Combine(SyncCertFolderPath, @"MPSC.cert");
            }
        }

        public string SyncServerProtocol {
            get {
                return Preferences.Get(nameof(SyncServerProtocol), @"https://");
            }
            set {
                Preferences.Set(nameof(SyncServerProtocol), value);
            }
        }

        public string SyncServerHostNameOrIp {
            get {
                return Preferences.Get(nameof(SyncServerHostNameOrIp), @"monkeypaste.com");
            }
            set {
                Preferences.Set(nameof(SyncServerHostNameOrIp), value);
            }
        }

        public int SyncServerPort {
            get {
                return Preferences.Get(nameof(SyncServerPort), 44376);
            }
            set {
                Preferences.Set(nameof(SyncServerPort), value);
            }
        }

        public string SyncServerEndpoint {
            get {
                return $"{SyncServerProtocol}{SyncServerHostNameOrIp}:{SyncServerPort}";
            }
        }
        #endregion

        #region User Properties
        public string ThisClientGuidStr {
            get {
                return Preferences.Get(nameof(ThisClientGuidStr), Guid.NewGuid().ToString());
            }
            set {
                Preferences.Set(nameof(ThisClientGuidStr), value);
            }
        }

        public string SslPrivateKey {
            get {
                return Preferences.Get(nameof(SslPrivateKey), string.Empty);
            }
            set {
                Preferences.Set(nameof(SslPrivateKey), value);
            }
        }

        public string SslPublicKey {
            get {
                return Preferences.Get(nameof(SslPublicKey), string.Empty);
            }
            set {
                Preferences.Set(nameof(SslPublicKey), value);
            }
        }

        public DateTime SslCertExpirationDateTime {
            get {
                return Preferences.Get(nameof(SslCertExpirationDateTime), DateTime.UtcNow.AddDays(-1));
            }
            set {
                Preferences.Set(nameof(SslCertExpirationDateTime), value);
            }
        }

        public bool EncryptDb {
            get {
                return Preferences.Get(nameof(EncryptDb), true);
            }
            set {
                Preferences.Set(nameof(EncryptDb), value);
            }
        }

        public string DbPassword {
            get {
                return Preferences.Get(
                    nameof(DbPassword), 
                    MpHelpers.Instance.GetRandomString(
                        MpHelpers.Instance.Rand.Next(
                            MinDbPasswordLength, 
                            MaxDbPasswordLength), 
                        MpHelpers.Instance.PasswordChars));
            }
        }

        public bool IsSearchCaseSensitive {
            get {
                return Preferences.Get(nameof(IsSearchCaseSensitive), false);
            }
            set {
                Preferences.Set(nameof(IsSearchCaseSensitive), value);
            }
        }

        public string UserName {
            get {
                return Preferences.Get(nameof(UserName), "Not Set");
            }
            set {
                Preferences.Set(nameof(UserName), value);
            }
        }

        public string UserEmail {
            get {
                return Preferences.Get(nameof(UserEmail), "tkefauver@gmail.com");
            }
            set {
                Preferences.Set(nameof(UserEmail), value);
            }
        }

        public int SyncPort {
            get {
                return Preferences.Get(nameof(UserName), 11000);
            }
            set {
                Preferences.Set(nameof(UserName), value);
            }
        }
        #endregion


        #endregion
    }
}
