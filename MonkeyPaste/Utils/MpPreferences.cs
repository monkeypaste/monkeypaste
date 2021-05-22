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
        public string LocalStoragePath {
            get {
                return Preferences.Get(nameof(DbPath), Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)); 
            }
        }

        public string DbPath {
            get {
                return Preferences.Get(nameof(DbPath), MpDbConstants.DbPath);
            }
            set {
                Preferences.Set(nameof(DbPath), value);
            }
        }

        public string DbName {
            get {
                return Preferences.Get(nameof(DbName), MpDbConstants.DbName);
            }
            set {
                Preferences.Set(nameof(DbName), value);
            }
        }

        public string DbMediaFolderPath {
            get {
                return Preferences.Get(nameof(DbMediaFolderPath), Path.Combine(LocalStoragePath,"media"));
            }
            set {
                Preferences.Set(nameof(DbMediaFolderPath), value);
            }
        }
        #endregion
    }
}
