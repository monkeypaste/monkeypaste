﻿
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

namespace MonkeyPaste.Avalonia.Android {
    [Activity(
    Label = "MonkeyPaste.Avalonia.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class MainActivity : AvaloniaMainActivity {
        private bool _isFullscreen = false;
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            if (_isFullscreen) {
                SetFullscreenWindowLayout();
            }

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

        public override void OnWindowFocusChanged(bool hasFocus) {
            base.OnWindowFocusChanged(hasFocus);

            MpAvMainWindowViewModel.Instance.IsMainWindowActive = hasFocus;
            if (hasFocus && _isFullscreen) {
                SetFullscreenWindowLayout();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1422:Validate platform compatibility", Justification = "<Pending>")]
        private void SetFullscreenWindowLayout() {
            if (Window != null) {
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

                    Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.Fullscreen |
                                                                           SystemUiFlags.HideNavigation |
                                                                           SystemUiFlags.Immersive |
                                                                           SystemUiFlags.ImmersiveSticky |
                                                                           SystemUiFlags.LayoutHideNavigation |
                                                                           SystemUiFlags.LayoutStable |
                                                                           SystemUiFlags.LowProfile);
                }
            }
        }
    }
}