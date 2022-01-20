using System.IO;

namespace MpWpfApp {
    public class MpWpfDbInfo : MonkeyPaste.MpIDbInfo {
        public string GetDbFilePath() {
            if (string.IsNullOrEmpty(Properties.Settings.Default.DbPath) ||
                !File.Exists(Properties.Settings.Default.DbPath)) {
                MonkeyPaste.MpConsole.WriteLine("Db does not exist in " + MpHelpers.GetApplicationDirectory());
                Properties.Settings.Default.DbPath = MpHelpers.GetApplicationDirectory() + Properties.Settings.Default.DbName;
                Properties.Settings.Default.DbPassword = string.Empty;
                Properties.Settings.Default.Save();
                //MpHelpers.WriteTextToFile(Properties.Settings.Default.DbPath, string.Empty);
                //SQLiteConnection.CreateFile(Properties.Settings.Default.DbPath);
            }
            return Properties.Settings.Default.DbPath;
        }

        public string GetDbName() {
            return "Mp.db";
        }

        public string GetDbPassword() {
            return string.Empty;
        }
    }
}