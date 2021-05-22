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
                MpConsole.WriteLine(@"PROCESS_TEXT: " + selectedText.ToString());

                var intent = new Intent(this, typeof(MainActivity));
                intent.PutExtra("SelectedText", selectedText);
                intent.PutExtra("HostPackageName", this.Referrer.Host);

                if (!string.IsNullOrEmpty(this.Referrer.Host)) {
                    try {
                        var appInfo = this.ApplicationContext.PackageManager.GetApplicationInfo(this.Referrer.Host, 0);
                        var appName = appInfo != null ? this.ApplicationContext.PackageManager.GetApplicationLabel(appInfo) : "(unknown)";
                        intent.PutExtra("HostAppName", appName);
                    } catch (Exception ex) {
                        MpConsole.WriteLine(@"Android.ProcessTextActivity error finding app name for referrer: " + this.Referrer.Host);
                        MpConsole.WriteLine(@"With Exception: " + ex);
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

        public IImageSourceHandler GetHandler(ImageSource source) {
            //Image source handler to return 
            IImageSourceHandler returnValue = null;
            //check the specific source type and return the correct image source handler 
            if (source is UriImageSource) {
                returnValue = new ImageLoaderSourceHandler();
            } else if (source is FileImageSource) {
                returnValue = new FileImageSourceHandler();
            } else if (source is StreamImageSource) {
                returnValue = new StreamImagesourceHandler();
            }
            return returnValue;
        }
        public async Task<Bitmap> GetBitmapFromImageSourceAsync(ImageSource source, Context context) {
            var handler = GetHandler(source);
            var returnValue = (Bitmap)null;
            returnValue = await handler.LoadImageAsync(source, context);
            return returnValue;
        }

        private async Task<Bitmap> GetBitmap(Xamarin.Forms.ImageSource imgSrc) {
            var handler = new ImageLoaderSourceHandler();
            var bmp = await handler.LoadImageAsync(imgSrc, this.ApplicationContext);
            return bmp;
        }
        private byte[] GetByteArray(Bitmap bmp) {
            using (var stream = new MemoryStream()) {
                bmp.Compress(Bitmap.CompressFormat.Png, 0, stream);
                return stream.ToArray();
            }
        }
    }
}