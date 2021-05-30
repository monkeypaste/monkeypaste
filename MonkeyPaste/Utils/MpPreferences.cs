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

        public string DbPath {
            get {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return System.IO.Path.Combine(basePath, DbName);
            }
        }
        public string LocalStoragePath {
            get {
                return Preferences.Get(nameof(DbPath), Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
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
        #endregion


        #endregion
    }
}
