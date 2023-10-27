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

        private string _dbPassword;
        public string DbPassword {
            get {
                if (string.IsNullOrEmpty(_dbPassword)) {
                    if (MpAvPrefViewModel.Instance.RememberedDbPassword != null) {
                        return MpAvPrefViewModel.Instance.RememberedDbPassword;
                    }
                    if (DbCreateDateTime == null) {
                        DbCreateDateTime = new FileInfo(DbPath).CreationTimeUtc;
                    }
                    return DbCreateDateTime.ToStringOrEmpty();
                }
                return _dbPassword;
            }
            private set {
                if (_dbPassword != value) {
                    _dbPassword = value;
                }
            }
        }
        public bool HasUserDefinedPassword =>
            !string.IsNullOrEmpty(_dbPassword);

        public DateTime? DbCreateDateTime {
            get =>
                MpAvPrefViewModel.Instance == null ?
                null :
                MpAvPrefViewModel.Instance.DbCreateDateTime;
            set => MpAvPrefViewModel.Instance.DbCreateDateTime = value;
        }
        public void SetPassword(string pwd, bool remember) {
            MpAvPrefViewModel.Instance.RememberedDbPassword = remember ? pwd : null;
            DbPassword = pwd;
        }
    }
}