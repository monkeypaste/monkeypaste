
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.Core.App;
using Avalonia;
using Avalonia.Android;
using Avalonia.ReactiveUI;
using Avalonia.WebView.Android;
using DryIoc.FastExpressionCompiler.LightExpression;
using MonkeyPaste.Common;
using System.Linq;
using Orientation = Android.Content.Res.Orientation;

namespace MonkeyPaste.Avalonia.Android {
    [Activity(
    Label = "MonkeyPaste",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    LaunchMode = LaunchMode.SingleTop,
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
    public class MainActivity : AvaloniaMainActivity<App> {
        public static bool IsFullscreen => false;

        private static MainActivity _instance;
        public static MainActivity Instance =>
            _instance;
        public MainActivity() {
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }

        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) {
            _instance = this;

            return base.CustomizeAppBuilder(builder)
                 .WithInterFont()
                 .UseReactiveUI()
                 .UseAndroidWebView()
                 .AfterSetup(_ => {
                     new MpAvAdWrapper().CreateDeviceInstance(this);
                     //MpAvNativeWebViewHost.Implementation = new MpAvAdWebViewBuilder();
                 });
        }

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            if (IsFullscreen) {
                SetFullscreenWindowLayout();
            }
            DoPermissionCheck();
            MpAvAdUncaughtExceptionHandler.Instance.Init();
        }
        public override void OnBackPressed() {
            if (MpAvApplicationCommand.Instance.BackNavCommand.CanExecute(null)) {
                MpAvApplicationCommand.Instance.BackNavCommand.Execute(null);
                return;
            }

            base.OnBackPressed();
        }

        public override void OnConfigurationChanged(Configuration newConfig) {
            base.OnConfigurationChanged(newConfig);

            MpAvDeviceWrapper.Instance.PlatformToastNotification
                .ShowToast(string.Empty, newConfig.Orientation.ToString(), null, null);

            var display = this.WindowManager.DefaultDisplay;
            MpMainWindowOrientationType mwot = display.Rotation.ToPortableOrientationType();
            MpAvMainWindowViewModel.Instance.CycleOrientationCommand.Execute(mwot);
        }


        public override void OnWindowFocusChanged(bool hasFocus) {
            base.OnWindowFocusChanged(hasFocus);
            MpAvMainWindowViewModel.Instance.IsMainWindowActive = hasFocus;
            if (hasFocus && IsFullscreen) {
                SetFullscreenWindowLayout();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1422:Validate platform compatibility", Justification = "<Pending>")]
        private void SetFullscreenWindowLayout() {
            if (Window == null) {
                return;
            }
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R) {
                IWindowInsetsController wicController = Window.InsetsController;
                Window.SetDecorFitsSystemWindows(false);
                Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
                if (wicController != null) {
                    wicController.Hide(WindowInsets.Type.Ime());
                    wicController.Hide(WindowInsets.Type.NavigationBars());
                }
            } else {

                Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

                Window.DecorView.SystemUiFlags = SystemUiFlags.Fullscreen |
                                                   SystemUiFlags.HideNavigation |
                                                   SystemUiFlags.Immersive |
                                                   SystemUiFlags.ImmersiveSticky |
                                                   SystemUiFlags.LayoutHideNavigation |
                                                   SystemUiFlags.LayoutStable |
                                                   SystemUiFlags.LowProfile;
            }
        }

        public bool DoPermissionCheck() {
            // from https://stackoverflow.com/a/33162451/105028

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M) {
                
                if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted) {
                    return true;
                }
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReadExternalStorage }, 1);
                return false;
            } else {
                return true;
            }
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults) {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            MpConsole.WriteLine($"Permissions: {string.Join(",", permissions)}");
            MpConsole.WriteLine($"Results    : {string.Join(",", grantResults.Select(x => x.ToString()))}");
        }

        //protected override void OnResume() {
        //    base.OnResume();

        //    StartActivity(new Intent(Application.Context, typeof(MainActivity)));

        //    Finish();
        //}

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.MainWindowLoadComplete:
                    //ForegroundService.Instance.Start();
                    break;
            }
        }

    }
}
