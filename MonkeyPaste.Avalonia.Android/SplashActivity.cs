using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Avalonia;
using Avalonia.Android;
using ControlCatalog.Android;
using System;
using Xamarin.Essentials;
using static Android.Views.ViewTreeObserver;
using Application = Android.App.Application;

namespace MonkeyPaste.Avalonia.Android {
    [Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : AvaloniaSplashActivity<App> {
        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) {

            return base.CustomizeAppBuilder(builder)
                 .AfterSetup(_ => {
                     WebView.SetWebContentsDebuggingEnabled(true);
                     new MpAvAdWrapper().CreateDeviceInstance(this);
                     MpAvNativeWebViewHost.Implementation = new MpAvAdWebViewBuilder();
                 });
        }

        protected override void OnPause() {
            base.OnPause();
        }
        protected override void OnResume() {
            base.OnResume();

            StartActivity(new Intent(Application.Context, typeof(MainActivity)));

            Finish();
        }
    }
}