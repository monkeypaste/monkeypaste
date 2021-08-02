using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Droid {
    [Activity(
        Label = "Monkey Paste", 
        Theme = "@style/Splash", 
        MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity {
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            var startup = new Intent(this, typeof(MainActivity));
            StartActivity(startup);
            Finish();
        }
    }
}