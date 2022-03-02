using MonkeyPaste;
using System.IO;

namespace MpWpfApp {
    public class MpWpfDbInfo : MonkeyPaste.MpIDbInfo {
        public string GetDbFilePath() {
            if (string.IsNullOrEmpty(MpPreferences.DbPath) ||
                !File.Exists(MpPreferences.DbPath)) {
                MonkeyPaste.MpConsole.WriteLine("Db does not exist in " + MpHelpers.GetApplicationDirectory());
                MpPreferences.DbPath = MpHelpers.GetApplicationDirectory() + MpPreferences.DbName;
                MpPreferences.DbPassword = string.Empty;
            }
            return MpPreferences.DbPath;
        }

        public string GetDbName() {
            return "Mp.db";
        }

        public string GetDbPassword() {
            return string.Empty;
        }
    }
}