using MonkeyPaste.UWP;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Windows.Storage;
using Xamarin.Forms;

[assembly: Dependency(typeof(MpDbFilePath_Uwp))]
namespace MonkeyPaste.UWP {
    public class MpDbFilePath_Uwp : MonkeyPaste.MpIDbFilePath {
        public string DbFilePath() {
            var dbName = MonkeyPaste.MpPreferences.Instance.DbName;
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, dbName);
        }
    }
}