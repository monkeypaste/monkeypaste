using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Xamarin.Essentials;
using FFImageLoading.Forms.Platform;

namespace MonkeyPaste.Droid {
    [Activity(
        Label = "MonkeyPaste",
        Icon = "@mipmap/icon",
        Theme = "@style/MainTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {
        public static MainActivity Current;

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            CachedImageRenderer.Init(enableFastRenderer: true);

            _ = new MpBootstrapper();

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            CachedImageRenderer.InitImageViewHandler();

            Current = this;

            LoadApplication(new App());

            LoadSelectedTextAsync();
        }

        private async void LoadSelectedTextAsync() {
            var selectedText = Intent!.GetStringExtra("SelectedText");// ?? string.Empty;
            var hostInfo = Intent!.GetStringExtra("HostInfo") ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(selectedText)) {
                await Clipboard.SetTextAsync(selectedText);
                var cicvm = MpResolver.Resolve<MpCopyItemCollectionViewModel>();
                cicvm.AddItemFromClipboardCommand.Execute(new object[] { hostInfo, selectedText });
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == 33)
            {
                var importer = (MpPhotoImporter)MpResolver.Resolve<MpIPhotoImporter>();
                importer.ContinueWithPermission(true);// grantResults == null || grantResults.Length == 0 || (Permission)grantResults[0] == Permission.Granted);
            }
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

    }
}