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
using Java.Nio;
using Plugin.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace MonkeyPaste.Droid {
    [Activity(Label = "Monkey Copy", NoHistory = true)]
    [IntentFilter(
        new[] { Intent.ActionProcessText },
        Categories = new[] { Intent.CategoryDefault },
        DataMimeType =  "text/plain")]
    public class MpProcessTextActivity : Activity {

        protected override async void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            var selectedText = Intent.GetCharSequenceExtra(Intent.ExtraProcessText);
            if (!string.IsNullOrEmpty(selectedText)) {
                MpConsole.WriteTraceLine(@"PROCESS_TEXT: " + selectedText.ToString());

                var intent = new Intent(this, typeof(MainActivity));
                intent.PutExtra("SelectedText", selectedText);
                intent.PutExtra("HostPackageName", this.Referrer.Host);

                if (!string.IsNullOrEmpty(this.Referrer.Host)) {
                    try {
                        var appInfo = this.ApplicationContext.PackageManager.GetApplicationInfo(this.Referrer.Host, 0);
                        var appName = appInfo != null ? this.ApplicationContext.PackageManager.GetApplicationLabel(appInfo) : "(unknown)";
                        intent.PutExtra("HostAppName", appName);
                    } catch (Exception ex) {
                        MpConsole.WriteTraceLine(@"Android.ProcessTextActivity error finding app name for referrer: " + this.Referrer.Host);
                        MpConsole.WriteTraceLine(@"With Exception: " + ex);
                    }                    
                    var imgSrc = MpPackageNameSource.FromPackageName(this.Referrer.Host);
                    var pnsh = new MpPackageNameSourceHandler();
                    var bmp = await pnsh.LoadImageAsync(imgSrc, this.ApplicationContext);
                    byte[] imageInByte = GetByteArray(bmp);
                    if (imageInByte != null && imageInByte.Length > 0) {
                        intent.PutExtra("HostIconByteArray", imageInByte);
                    }
                }
                
                StartActivity(intent);
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