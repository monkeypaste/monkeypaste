using System;
using System.Collections.Generic;
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
        public string DbPath {
            get {
                return Preferences.Get(nameof(DbPath), "Unknown");
            }
            set {
                Preferences.Set(nameof(DbPath), value);
            }
        }
        #endregion
    }
}
