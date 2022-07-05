using MonkeyPaste;
using System.IO;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using System.Reflection;

namespace MonkeyPaste.Avalonia {
    public class MpAvDbInfo : MonkeyPaste.MpIDbInfo {
        public string GetDbFilePath() {
            //string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //if (string.IsNullOrEmpty(MpJsonPreferenceIO.Instance.DbPath) ||
            //    !File.Exists(MpJsonPreferenceIO.Instance.DbPath)) {
            //    MpConsole.WriteLine("Db does not exist in " + appDir);
            //    MpJsonPreferenceIO.Instance.DbPath = Path.Combine(appDir,MpJsonPreferenceIO.Instance.DbName);
            //    MpJsonPreferenceIO.Instance.DbPassword = string.Empty;
            //}
            //return MpJsonPreferenceIO.Instance.DbPath;
            return MpPrefViewModel.Instance.DbPath;
        }

        public string GetDbName() {
            return "mp.db";
        }

        public string GetDbPassword() {
            return string.Empty;
        }

        public MpAvDbInfo() { }
    }
}