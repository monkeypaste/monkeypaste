using MonkeyPaste;
using System.IO;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using System.Reflection;
using System;

namespace MonkeyPaste.Avalonia {
    public class MpAvDbInfo : MonkeyPaste.MpIDbInfo {
        public string DbName {
            get {                
                if(OperatingSystem.IsWindows()) {
                    return "mp_win.db";
                } 
                if(OperatingSystem.IsLinux()) {
                    return "mp_x11.db";
                }
                if(OperatingSystem.IsMacOS()) {
                    return "mp_mac.db";
                }
                throw new Exception("Unknown os");

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