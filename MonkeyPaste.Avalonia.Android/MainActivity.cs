
using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Widget;

using Avalonia.Android;
using Avalonia.Threading;
using Orientation = Android.Content.Res.Orientation;

namespace MonkeyPaste.Avalonia.Android;
[Activity(
    Label = "MonkeyPaste.Avalonia.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
public class MainActivity : AvaloniaMainActivity {
    public override void OnConfigurationChanged(Configuration newConfig) {
        base.OnConfigurationChanged(newConfig);

        var test = App.MainView;
        // Checks the orientation of the screen
        if (newConfig.Orientation == Orientation.Landscape) {
            Toast.MakeText(this, "landscape", ToastLength.Short).Show();
        } else if (newConfig.Orientation == Orientation.Portrait) {
            Toast.MakeText(this, "portrait", ToastLength.Short).Show();
        }
    }
}
