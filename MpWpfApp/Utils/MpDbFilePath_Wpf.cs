using MonkeyPaste;
using MpWpfApp;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

[assembly: Xamarin.Forms.Dependency(typeof(MpDbFilePath_Wpf))]
namespace MpWpfApp {
    public class MpDbFilePath_Wpf : MonkeyPaste.MpIDbFilePath {
        public string DbFilePath() {
            if (string.IsNullOrEmpty(Properties.Settings.Default.DbPath) ||
                !File.Exists(Properties.Settings.Default.DbPath)) {
                Console.WriteLine("Db does not exist in " + MpHelpers.Instance.GetApplicationDirectory());
                Properties.Settings.Default.DbPath = MpHelpers.Instance.GetApplicationDirectory() + Properties.Settings.Default.DbName;
                Properties.Settings.Default.DbPassword = string.Empty;
                Properties.Settings.Default.Save();
                SQLiteConnection.CreateFile(Properties.Settings.Default.DbPath);
            }
            return Properties.Settings.Default.DbPath;
        }
    }
}