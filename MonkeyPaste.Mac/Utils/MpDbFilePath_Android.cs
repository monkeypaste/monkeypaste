
using SQLite;
using System;
using System.IO;
using System.Linq;
using System.Text;

[assembly: Xamarin.Forms.Dependency(typeof(MonkeyPaste.Mac.MpDbFilePath_Mac))]
namespace MonkeyPaste.Mac {
    public class MpDbFilePath_Mac : MonkeyPaste.MpIDbInfo {
        public string GetDbFilePath() {
            return Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), 
                MonkeyPaste.MpPreferences.Instance.DbName);
        }

        public string GetDbName() {
            return "Mp.db";
        }

        public string GetDbPassword() {
            return string.Empty;
        }
    }
}