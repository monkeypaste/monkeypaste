using MonkeyPaste;
using System.IO;
using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common;
using System.Reflection;
using System;

namespace MonkeyPaste.Avalonia {
    public class MpAvDbInfo : MonkeyPaste.MpIDbInfo {
        public string DbExtension => "mpcdb";
        public string DbName {
            get {
                string db_name_by_os = string.Empty;
                if(OperatingSystem.IsWindows()) {
                    db_name_by_os = "mp_win";
                } else if(OperatingSystem.IsLinux()) {
                    db_name_by_os = "mp_x11";
                } else if(OperatingSystem.IsMacOS()) {
                    db_name_by_os = "mp_mac";
                } else {
                    throw new Exception("Unmanaged os");
                }
                return $"{db_name_by_os}.{DbExtension}";
            }
        }
        public string DbPath => Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), DbName);
        //public string GetDbFilePath() {
        //    //string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //    //if (string.IsNullOrEmpty(MpJsonPreferenceIO.Instance.DbPath) ||
        //    //    !File.Exists(MpJsonPreferenceIO.Instance.DbPath)) {
        //    //    MpConsole.WriteLine("Db does not exist in " + appDir);
        //    //    MpJsonPreferenceIO.Instance.DbPath = Path.Combine(appDir,MpJsonPreferenceIO.Instance.DbName);
        //    //    MpJsonPreferenceIO.Instance.DbPassword = string.Empty;
        //    //}
        //    //return MpJsonPreferenceIO.Instance.DbPath;
        //    return MpPrefViewModel.Instance.DbPath;
        //}

        //public string GetDbName() {
        //    return "mp.db";
        //}

        //public string GetDbPassword() {
        //    return string.Empty;
        //}

        //public MpAvDbInfo() { }
    }
}