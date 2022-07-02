using MonkeyPaste;
using System.IO;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using System.Reflection;

namespace MonkeyPaste.Avalonia {
    public class MpAvDbInfo : MonkeyPaste.MpIDbInfo {
        public string GetDbFilePath() {
            string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(MpPreferences.DbPath) ||
                !File.Exists(MpPreferences.DbPath)) {
                MpConsole.WriteLine("Db does not exist in " + appDir);
                MpPreferences.DbPath = Path.Combine(appDir,MpPreferences.DbName);
                MpPreferences.DbPassword = string.Empty;
            }
            return MpPreferences.DbPath;
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