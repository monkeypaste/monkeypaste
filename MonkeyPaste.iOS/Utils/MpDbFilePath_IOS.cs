using MonkeyPaste.iOS;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

[assembly: Xamarin.Forms.Dependency(typeof(MpDbFilePath_iOS))]
namespace MonkeyPaste.iOS {
    public class MpDbFilePath_iOS : MonkeyPaste.MpIDbInfo {
        public string GetDbFilePath() {
            string personalFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string libraryFolder = Path.Combine(personalFolder, "..", "Library");
            return Path.Combine(libraryFolder, MonkeyPaste.MpPreferences.DbName);
        }
    }
}