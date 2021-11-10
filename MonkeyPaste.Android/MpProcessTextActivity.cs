using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using FFImageLoading.Forms;
using Java.IO;
using Java.Lang;
using Java.Nio;
using Plugin.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace MonkeyPaste.Droid {
    [Activity(Label = "Monkey Copy", NoHistory = true)]
    [IntentFilter(
        new[] { Intent.ActionProcessText },
        Categories = new[] { Intent.CategoryDefault },
        DataMimeType =  "text/plain")]
    public class MpProcessTextActivity : Activity {
        public static MpProcessTextActivity Current;

        public static Window GetWindow() {
            if(Current == null) {
                return null;
            }
            return Current.Window;
        }

        protected override async void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            string selectedText = string.Empty, hostPackageName = string.Empty, hostAppName = string.Empty, hostAppIconBase64 = string.Empty;
            byte[] hostIconByteArray = null;

            selectedText = Intent.GetCharSequenceExtra(Intent.ExtraProcessText);

            if (!string.IsNullOrEmpty(selectedText)) {
                hostPackageName = this.Referrer.Host;

                if (!string.IsNullOrEmpty(this.Referrer.Host)) {
                    try {
                        var appInfo = this.ApplicationContext.PackageManager.GetApplicationInfo(this.Referrer.Host, 0);
                        hostAppName = appInfo != null ? this.ApplicationContext.PackageManager.GetApplicationLabel(appInfo) : "(unknown)";
                    } catch (System.Exception ex) {
                        MpConsole.WriteTraceLine(@"Android.ProcessTextActivity error finding app name for referrer: " + this.Referrer.Host);
                        MpConsole.WriteTraceLine(@"With Exception: " + ex);
                    }                    
                    var imgSrc = MpPackageNameSource.FromPackageName(this.Referrer.Host);
                    var pnsh = new MpPackageNameSourceHandler();
                    var bmp = await pnsh.LoadImageAsync(imgSrc, this.ApplicationContext);
                    hostIconByteArray = GetByteArray(bmp);
                    if (hostIconByteArray != null && hostIconByteArray.Length > 0) {
                        hostAppIconBase64 = Convert.ToBase64String(hostIconByteArray);
                    }
                }
                if (!string.IsNullOrWhiteSpace(selectedText)) {
                    await Clipboard.SetTextAsync(selectedText);

                    // TODO Add Skia Icon builder here
                    var icon = await MpIcon.Create(hostAppIconBase64);
                    var app = await MpApp.Create(hostPackageName,hostAppName,icon);
                    var source = await MpSource.Create(app, null);
                    await MpCopyItem.Create(source, selectedText,MpCopyItemType.RichText);
                        //new object[] { hostPackageName, selectedText, hostAppName, hostIconByteArray, hostAppIconBase64 });
                }
                
                StartActivity(new Intent(this, typeof(MainActivity)));
                Finish();
            }            
        }

        private byte[] GetByteArray(Bitmap bmp) {
            using (var stream = new MemoryStream()) {
                bmp.Compress(Bitmap.CompressFormat.Png, 0, stream);
                return stream.ToArray();
            }
        }
    }
}