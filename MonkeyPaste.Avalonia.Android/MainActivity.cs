
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Avalonia.Android;
using Avalonia.Threading;
using Com.Xamarin.Formsviewgroup;
using Orientation = Android.Content.Res.Orientation;

namespace MonkeyPaste.Avalonia.Android;
[Activity(
    Label = "MonkeyPaste.Avalonia.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
public class MainActivity : AvaloniaMainActivity {

    protected override void OnCreate(Bundle savedInstanceState) {
        base.OnCreate(savedInstanceState);

        WebView.SetWebContentsDebuggingEnabled(BuildConfig.Debug);
        MpAvAdUncaughtExceptionHandler.Instance.Init();
    }
    public override void OnConfigurationChanged(Configuration newConfig) {
        base.OnConfigurationChanged(newConfig);

        if (newConfig.Orientation == Orientation.Landscape) {
            Toast.MakeText(this, "landscape", ToastLength.Short).Show();
        } else if (newConfig.Orientation == Orientation.Portrait) {
            Toast.MakeText(this, "portrait", ToastLength.Short).Show();
        }

        var display = this.WindowManager.DefaultDisplay;
        MpMainWindowOrientationType mwot = display.Rotation.ToPortableOrientationType();
        MpAvMainWindowViewModel.Instance.CycleOrientationCommand.Execute(mwot);

    }
}
