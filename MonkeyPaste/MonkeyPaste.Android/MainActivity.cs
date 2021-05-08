using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Xamarin.Essentials;
using MonkeyPaste.ViewModels;

namespace MonkeyPaste.Droid {
    [Activity(
        Label = "MonkeyPaste",
        Icon = "@mipmap/icon",
        Theme = "@style/MainTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            MpBootstrapper.Init();

            LoadApplication(new App());
            LoadSelectedTextAsync();
        }

        private async void LoadSelectedTextAsync() {
            var text = Intent!.GetStringExtra("selectedText");
            if (!string.IsNullOrWhiteSpace(text)) {
                await Clipboard.SetTextAsync(text);

                var mvm = MpResolver.Resolve<MpMainViewModel>();
                mvm.AddItemFromClipboardCommand.Execute(text);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults) {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}