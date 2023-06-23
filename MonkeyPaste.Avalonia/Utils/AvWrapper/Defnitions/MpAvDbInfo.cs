using MonkeyPaste;
using MonkeyPaste.Common;
using System;
using System.IO;
using System.Reflection;

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

        public string DbPassword =>
            MpPrefViewModel.Instance == null ?
                null :
                MpPrefViewModel.Instance.DbPassword;
    }
}