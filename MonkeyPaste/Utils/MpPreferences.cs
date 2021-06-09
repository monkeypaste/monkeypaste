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

        public static string SyncServerProtocol {
            get {
                return Preferences.Get(nameof(SyncServerProtocol), @"https://");
            }
            set {
                Preferences.Set(nameof(SyncServerProtocol), value);
            }
        }

        public static string SyncServerHostNameOrIp {
            get {
                return Preferences.Get(nameof(SyncServerHostNameOrIp), @"192.168.43.209");
            }
            set {
                Preferences.Set(nameof(SyncServerHostNameOrIp), value);
            }
        }

        public static int SyncServerPort {
            get {
                return Preferences.Get(nameof(SyncServerPort), 44376);
            }
            set {
                Preferences.Set(nameof(SyncServerPort), value);
            }
        }

        public static string SyncServerEndpoint {
            get {
                return $"{SyncServerProtocol}{SyncServerHostNameOrIp}:{SyncServerPort}";
            }
        }
        #endregion

        #region User Properties
        public static bool EncryptDb {
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

        public static bool IsSearchCaseSensitive {
            get {
                return Preferences.Get(nameof(IsSearchCaseSensitive), false);
            }
            set {
                Preferences.Set(nameof(IsSearchCaseSensitive), value);
            }
        }

        public static string UserName {
            get {
                return Preferences.Get(nameof(UserName), "Not Set");
            }
            set {
                Preferences.Set(nameof(UserName), value);
            }
        }

        public static string UserEmail {
            get {
                return Preferences.Get(nameof(UserEmail), "tkefauver@gmail.com");
            }
            set {
                Preferences.Set(nameof(UserEmail), value);
            }
        }

        public static int SyncPort {
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
