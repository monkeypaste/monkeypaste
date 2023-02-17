using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using ControlCatalog.Android;
using Application = Android.App.Application;

namespace MonkeyPaste.Avalonia.Android;

[Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
public class SplashActivity : AvaloniaSplashActivity<App> {
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) {
        return base.CustomizeAppBuilder(builder)
             .AfterSetup(_ => {
                 //MpAvNativeWebViewHost.Implementation = new MpAvAdWebView();
             });
    }

    protected override void OnCreate(Bundle? savedInstanceState) {
        base.OnCreate(savedInstanceState);
    }

    protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data) {
        base.OnActivityResult(requestCode, resultCode, data);


        StartActivity(new Intent(Application.Context, typeof(MainActivity)));

        Finish();
    }

    protected override void OnResume() {
        base.OnResume();

        StartActivity(new Intent(Application.Context, typeof(MainActivity)));

        Finish();
    }
}
