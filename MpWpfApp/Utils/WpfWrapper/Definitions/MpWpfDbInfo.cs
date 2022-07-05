using MonkeyPaste;
using System.IO;
using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common; 
using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    public class MpWpfDbInfo : MonkeyPaste.MpIDbInfo {
        public string GetDbFilePath() {
            //if (string.IsNullOrEmpty(MpJsonPreferenceIO.Instance.DbPath) ||
            //    !File.Exists(MpJsonPreferenceIO.Instance.DbPath)) {
            //    MpConsole.WriteLine("Db does not exist in " + MpHelpers.GetApplicationDirectory());
            //    MpJsonPreferenceIO.Instance.DbPath = MpHelpers.GetApplicationDirectory() + MpJsonPreferenceIO.Instance.DbName;
            //    MpJsonPreferenceIO.Instance.DbPassword = string.Empty;
            //}
            return MpPrefViewModel.Instance.DbPath;
        }

        public string GetDbName() {
            return "mp.db";
        }

        public string GetDbPassword() {
            return string.Empty;
        }
    }
}