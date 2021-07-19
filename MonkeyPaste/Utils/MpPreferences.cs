using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Xamarin.Essentials;

namespace MonkeyPaste {
    public class MpPreferences : MpIPreferences {
        #region Singleton
        private static readonly Lazy<MpPreferences> _Lazy = new Lazy<MpPreferences>(() => new MpPreferences());
        public static MpPreferences Instance { get { return _Lazy.Value; } }

        private MpPreferences() {
            _prefDataTypeLookup = new Dictionary<string, Type>();
            foreach(var prop in GetType().GetProperties()) {
                _prefDataTypeLookup.Add(prop.Name, prop.PropertyType);
            }
        }
        #endregion

        #region Private Variables
        private Dictionary<string, Type> _prefDataTypeLookup;
        #endregion

        #region Properties


        #region Application Properties
        public string SslAlgorithm { get; set; } = "SHA256WITHRSA";
        public string SslCASubject { get; set; } = "CN{ get; set; } =MPCA";
        public string SslCertSubject { get; set; } = "CN{ get; set; } =127.0.01";

        public string DbName { get; set; } = "Mp.db";
        public int MinDbPasswordLength { get; set; } = 12;
        public int MaxDbPasswordLength { get; set; } = 18;

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
                return Preferences.Get(nameof(ThisClientGuidStr), string.Empty);
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

        #region MpIPreferences Implementation
        public object GetPreferenceValue(string preferenceName) {
            return this.GetType().GetProperty(preferenceName).GetValue(this);
        }

        public void SetPreferenceValue(string preferenceName, object preferenceValue) {
            this.GetType().GetProperty(preferenceName).SetValue(this, preferenceValue);
        }
        #endregion
    }
}
