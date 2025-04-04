﻿using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Text;
using Android.Util;

using Avalonia;
using Avalonia.Android;
using Avalonia.ReactiveUI;

namespace iosKeyboardTest.Android {
    [Activity(
    Label = "iosKeyboardTest.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
    public class MainActivity : AvaloniaMainActivity<App> {
        public static MainActivity Instance { get; private set; }

        public MainActivity() : base() { }
        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) {
            Instance = this;
            var result = base.CustomizeAppBuilder(builder)
                     .WithInterFont()
                     .UseReactiveUI()
                     .AfterSetup(_=> {
                         PlatformKeyboardServices.KeyboardPermissionHelper = new AndroidPermissionHelper();
                     });

            return result;
        }

    }
}
