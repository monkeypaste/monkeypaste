using MonkeyPaste.UWP;
using System.IO;
[assembly: Xamarin.Forms.Dependency(typeof(MpDbFilePath_Uwp))]
namespace MonkeyPaste.UWP {
    public class MpDbFilePath_Uwp : MonkeyPaste.MpIDbInfo {
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