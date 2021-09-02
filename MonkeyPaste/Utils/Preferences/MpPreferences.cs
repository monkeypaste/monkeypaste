using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpPreferences {
        #region Singleton
        private static readonly Lazy<MpPreferences> _Lazy = new Lazy<MpPreferences>(() => new MpPreferences());
        public static MpPreferences Instance { get { return _Lazy.Value; } }

        private MpPreferences() {
        }

        public void Init(MpIPreferenceIO prefIo) {
            _prefIo = prefIo;

            if (string.IsNullOrEmpty(ThisDeviceGuid)) {
                MpPreferences.Instance.ThisDeviceGuid = System.Guid.NewGuid().ToString();
            }
        }
        #endregion

        #region Private Variables

        private MpIPreferenceIO _prefIo;
        #endregion

        #region Properties


        #region Application Properties
        public string SslAlgorithm { get; set; } = "SHA256WITHRSA";
        public string SslCASubject { get; set; } = "CN{ get; set; } =MPCA";
        public string SslCertSubject { get; set; } = "CN{ get; set; } =127.0.01";

        public string DbName { get; set; } = "Mp.db";
        public int MinDbPasswordLength { get; set; } = 12;
        public int MaxDbPasswordLength { get; set; } = 18;

        public MpUserDeviceType ThisDeviceType {
            get {
                return _prefIo.GetDeviceType();
            }
        }

        public string LocalStoragePath {
            get {
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
        }

        public string DbPath {
            get {
                return _prefIo.Get(nameof(DbPath), Path.Combine(LocalStoragePath, DbName));
            }
        }

        public string DbMediaFolderPath {
            get {
                return _prefIo.Get(nameof(DbMediaFolderPath), Path.Combine(LocalStoragePath, "media"));
            }
            set {
                _prefIo.Set(nameof(DbMediaFolderPath), value);
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
                return _prefIo.Get(nameof(SyncServerProtocol), @"https://");
            }
            set {
                _prefIo.Set(nameof(SyncServerProtocol), value);
            }
        }

        public string SyncServerHostNameOrIp {
            get {
                return _prefIo.Get(nameof(SyncServerHostNameOrIp), @"monkeypaste.com");
            }
            set {
                _prefIo.Set(nameof(SyncServerHostNameOrIp), value);
            }
        }

        public int SyncServerPort {
            get {
                return _prefIo.Get(nameof(SyncServerPort), 44376);
            }
            set {
                _prefIo.Set(nameof(SyncServerPort), value);
            }
        }

        public string SyncServerEndpoint {
            get {
                return $"{SyncServerProtocol}{SyncServerHostNameOrIp}:{SyncServerPort}";
            }
        }
        #endregion

        #region User Properties
        public string FmsRegistrationToken {
            get {
                return _prefIo.Get(nameof(FmsRegistrationToken), string.Empty);
            }
            set {
                _prefIo.Set(nameof(FmsRegistrationToken), value);
            }
        }

        public string ThisDeviceGuid {
            get {
                return _prefIo.Get(nameof(ThisDeviceGuid), string.Empty);
            }
            set {
                _prefIo.Set(nameof(ThisDeviceGuid), value);
            }
        }

        public string SslPrivateKey {
            get {
                return _prefIo.Get(nameof(SslPrivateKey), string.Empty);
            }
            set {
                _prefIo.Set(nameof(SslPrivateKey), value);
            }
        }

        public string SslPublicKey {
            get {
                return _prefIo.Get(nameof(SslPublicKey), string.Empty);
            }
            set {
                _prefIo.Set(nameof(SslPublicKey), value);
            }
        }

        public DateTime SslCertExpirationDateTime {
            get {
                return _prefIo.Get(nameof(SslCertExpirationDateTime), DateTime.UtcNow.AddDays(-1));
            }
            set {
                _prefIo.Set(nameof(SslCertExpirationDateTime), value);
            }
        }

        public bool EncryptDb {
            get {
                return _prefIo.Get(nameof(EncryptDb), true);
            }
            set {
                _prefIo.Set(nameof(EncryptDb), value);
            }
        }

        public string DbPassword {
            get {
                return _prefIo.Get(
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
                return _prefIo.Get(nameof(IsSearchCaseSensitive), false);
            }
            set {
                _prefIo.Set(nameof(IsSearchCaseSensitive), value);
            }
        }

        public string UserName {
            get {
                return _prefIo.Get(nameof(UserName), "Not Set");
            }
            set {
                _prefIo.Set(nameof(UserName), value);
            }
        }

        public string UserEmail {
            get {
                return _prefIo.Get(nameof(UserEmail), "tkefauver@gmail.com");
            }
            set {
                _prefIo.Set(nameof(UserEmail), value);
            }
        }

        public int SyncPort {
            get {
                return _prefIo.Get(nameof(UserName), 11000);
            }
            set {
                _prefIo.Set(nameof(UserName), value);
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
