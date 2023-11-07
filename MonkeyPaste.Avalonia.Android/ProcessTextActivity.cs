using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Widget;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Threading.Tasks;
using Xamarin.Essentials;
using static Android.Content.PM.PackageManager;
using Intent = Android.Content.Intent;

namespace MonkeyPaste.Avalonia.Android {
    [Activity(Label = "Monkey Copy", NoHistory = true, Exported = true)]
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

            await Clipboard.SetTextAsync(selectedText);
            Toast.MakeText(this, "Copied to clipboard", ToastLength.Short).Show();

            string app_path = Referrer.Host;
            string app_name = GetAppName(app_path);
            string app_icon_base64 = await GetAppIconBase64Async(Referrer.Host);

            // TODO Add Skia Icon builder here

            var app_pi = new MpPortableProcessInfo() {
                ProcessPath = app_path,
                ApplicationName = app_name,
                MainWindowIconBase64 = app_icon_base64
            };
            var avdo = new MpAvDataObject(MpPortableDataFormats.Text, selectedText);
            avdo.SetData(MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT, app_pi);
            Mp.Services.ClipboardMonitor.ForceChange(avdo);


            //var source = await MpSource.CreateAsync(app, null);
            //StartActivity(new Intent(this, typeof(MainActivity)));
            Finish();
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        private string GetAppName(string packageName) {
            try {
                var pm = this.ApplicationContext.PackageManager;
                if (pm.GetApplicationInfo(packageName, ApplicationInfoFlags.Of(0)) is { } appInfo) {
                    return this.ApplicationContext.PackageManager.GetApplicationLabel(appInfo);
                }
            }
            catch (System.Exception ex) {
                MpConsole.WriteTraceLine(@"Android.ProcessTextActivity error finding app name for referrer: " + this.Referrer.Host);
                MpConsole.WriteTraceLine(@"With Exception: " + ex);
            }
            return "(unknown)";
        }
        private async Task<string> GetAppIconBase64Async(string packageName) {
            if (string.IsNullOrEmpty(packageName)) {
                return null;
            }
            using Drawable drawable = ApplicationContext.PackageManager.GetApplicationIcon(packageName);
            Bitmap bitmap = null;
            await Task.Run(() => {
                bitmap = Bitmap.CreateBitmap(drawable.IntrinsicWidth, drawable.IntrinsicHeight, Bitmap.Config.Argb8888);
                using (var canvas = new Canvas(bitmap)) {
                    drawable.SetBounds(0, 0, canvas.Width, canvas.Height);
                    drawable.Draw(canvas);
                }
            });
            return bitmap.ToBase64Str();
        }

    }
}
