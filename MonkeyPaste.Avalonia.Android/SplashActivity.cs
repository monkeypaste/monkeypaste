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
        //MpAvAdUncaughtExceptionHandler.Instance.Init();
        new MpAvAdWrapper().CreateDeviceInstance();

        return base.CustomizeAppBuilder(builder)
             .AfterSetup(_ => {
                 //Pages.EmbedSample.Implementation = new EmbedSampleAndroid();
                 MpAvNativeWebViewHost.Implementation = new MpAvAdWebViewBuilder();
                 MpAvNativeWebViewContainer.Implementation = new MpAvAdWebViewContainerBuilder();
             });
    }
    protected override void OnPause() {
        base.OnPause();
        //MpAvMainWindowViewModel.Instance.IsMainWindowActive = false;
    }
    protected override void OnResume() {
        base.OnResume();
        //MpAvMainWindowViewModel.Instance.IsMainWindowActive = true;

        StartActivity(new Intent(Application.Context, typeof(MainActivity)));

        Finish();
    }
}
