using MonkeyPaste.Common;
using System;
using System.IO;

namespace MonkeyPaste.Avalonia {
    public class MpAvDbInfo : MpIDbInfo {
        public string DbExtension =>
            "db";

        public string DbFileName =>
            $"mp.{DbExtension}";

        public string DbDir =>
            Mp.Services == null ||
            Mp.Services.PlatformInfo == null ||
            Mp.Services.PlatformInfo.StorageDir == null ?
                string.Empty :
                Mp.Services.PlatformInfo.StorageDir;

        public string DbPath =>
            Path.Combine(DbDir, DbFileName);
        public string DbPassword {
            get {
                if (string.IsNullOrEmpty(DbPassword2)) {
                    return DbPassword1;
                }
                return (DbPassword1 + DbPassword2).CheckSum();
            }
        }

        public string DbPassword1 {
            get {
                if (DbCreateDateTime == null) {
                    DbCreateDateTime = new FileInfo(DbPath).CreationTimeUtc;
                }
                return DbCreateDateTime.Value.ToTickChecksum();
            }
        }

        private string _dbPassword2;
        public string DbPassword2 {
            get {
                if (string.IsNullOrEmpty(_dbPassword2)) {
                    if (MpAvPrefViewModel.Instance.RememberedDbPassword != null) {
                        return MpAvPrefViewModel.Instance.RememberedDbPassword;
                    }
                }
                return _dbPassword2;
            }
            private set {
                if (_dbPassword2 != value) {
                    _dbPassword2 = value;
                }
            }
        }
        public bool HasUserDefinedPassword =>
            !string.IsNullOrEmpty(DbPassword2);

        public DateTime? DbCreateDateTime {
            get =>
                MpAvPrefViewModel.Instance == null ?
                null :
                MpAvPrefViewModel.Instance.DbCreateDateTime;
            set => MpAvPrefViewModel.Instance.DbCreateDateTime = value;
        }
        public void SetPassword(string pwd, bool remember) {
            MpAvPrefViewModel.Instance.RememberedDbPassword = remember ? pwd : null;
            DbPassword2 = pwd;
        }

        public string EnterPasswordTitle =>
            UiStrings.DbPasswordNtfTitle;
        public string EnterPasswordText =>
            UiStrings.DbPasswordNtfText;
    }
}