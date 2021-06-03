using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MonkeyPaste.Droid;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

[assembly: Xamarin.Forms.Dependency(typeof(MpDbFilePath_Android))]
namespace MonkeyPaste.Droid {
    public class MpDbFilePath_Android : MonkeyPaste.MpIDbFilePath {
        public string DbFilePath() {
            return Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), 
                MonkeyPaste.MpPreferences.DbName);
        }
    }
}