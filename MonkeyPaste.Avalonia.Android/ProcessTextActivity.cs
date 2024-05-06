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
            await Clipboard.SetTextAsync(selectedText);
            MpAvDeviceWrapper.Instance.PlatformToastNotification
                .ShowToast(string.Empty, UiStrings.MobileCopiedNtfText, null, null);

            CreateTextClip(selectedText);

            Finish();
        }

        private void CreateTextClip(string selectedText) {
            string package_name = Referrer.Host;
            string app_path = Path.Combine(Path.GetDirectoryName(DataDir.AbsolutePath), package_name);

            string app_name = GetAppName(package_name);
            string app_icon_base64 = Mp.Services.IconBuilder.GetPathIconBase64(app_path);

            var app_pi = new MpPortableProcessInfo() {
                ProcessPath = app_path,
                ApplicationName = app_name,
                MainWindowIconBase64 = app_icon_base64
            };
            var avdo = new MpAvDataObject(MpPortableDataFormats.Text, selectedText);
            avdo.SetData(MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT, app_pi);
            avdo.SetData(MpPortableDataFormats.INTERNAL_CONTENT_TYPE_FORMAT, MpCopyItemType.Text.ToString());

            Mp.Services.ContentBuilder.BuildFromDataObjectAsync(avdo, false, MpDataObjectSourceType.ClipboardWatcher).FireAndForgetSafeAsync();
            //var source = await MpSource.CreateAsync(app, null);
            //StartActivity(new Intent(this, typeof(MainActivity)));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        private string GetAppName(string packageName) {
            try {
                var pm = this.ApplicationContext.PackageManager;
                if (pm.GetApplicationInfo(packageName,0) is { } appInfo) {
                    return pm.GetApplicationLabel(appInfo);
                }
            }
            catch (System.Exception ex) {
                MpConsole.WriteTraceLine(@"Android.ProcessTextActivity error finding app name for referrer: " + this.Referrer.Host);
                MpConsole.WriteTraceLine(@"With Exception: " + ex);
            }
            return packageName;
        }

    }
}
