using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using Xamarin.Essentials;
using static Android.Content.PM.PackageManager;
using ApplicationInfoFlags = Android.Content.PM.PackageManager.ApplicationInfoFlags;
using Intent = Android.Content.Intent;
using Path = System.IO.Path;

namespace MonkeyPaste.Avalonia.Android {
    [Activity(
        Label = "Monkey Copy",
        NoHistory = true,
        Exported = true)]
    [IntentFilter(
        new[] { Intent.ActionProcessText },
        Categories = new[] { Intent.CategoryDefault },
        DataMimeType = "text/plain")]
    public class ProcessTextActivity : Activity {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        protected override async void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            string selectedText = Intent.GetCharSequenceExtra(Intent.ExtraProcessText);
            if (string.IsNullOrEmpty(selectedText)) {
                return;
            }
            string package_name = Referrer.Host;
            string app_path = Path.Combine(Path.GetDirectoryName(DataDir.AbsolutePath), package_name);

            string app_name = GetAppName(package_name);
            string app_icon_base64 = Mp.Services.IconBuilder.GetPathIconBase64(app_path);

            // TODO Add Skia Icon builder here

            var app_pi = new MpPortableProcessInfo() {
                ProcessPath = app_path,
                ApplicationName = app_name,
                MainWindowIconBase64 = app_icon_base64
            };
            var avdo = new MpAvDataObject(MpPortableDataFormats.Text, selectedText);
            avdo.SetData(MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT, app_pi);
            var avdo_ci = await Mp.Services.ContentBuilder.BuildFromDataObjectAsync(avdo, false);

            await Clipboard.SetTextAsync(selectedText);
            Toast.MakeText(this, "Copied to clipboard", ToastLength.Short).Show();


            //var source = await MpSource.CreateAsync(app, null);
            //StartActivity(new Intent(this, typeof(MainActivity)));

            Finish();
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        private string GetAppName(string packageName) {
            try {
                var pm = this.ApplicationContext.PackageManager;
                if (pm.GetApplicationInfo(packageName, ApplicationInfoFlags.Of(0)) is { } appInfo) {
                    return pm.GetApplicationLabel(appInfo);
                }
            }
            catch (System.Exception ex) {
                MpConsole.WriteTraceLine(@"Android.ProcessTextActivity error finding app name for referrer: " + this.Referrer.Host);
                MpConsole.WriteTraceLine(@"With Exception: " + ex);
            }
            return "(unknown)";
        }

    }
}
