using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Droid {
    [Activity(Label = "Monkey Copy", NoHistory = true)]
    [IntentFilter(
        new[] { Intent.ActionProcessText },
        Categories = new[] { Intent.CategoryDefault },
        DataMimeType =  "text/plain")]
    public class MpProcessTextActivity : Activity {

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            var selectedText = Intent.GetCharSequenceExtra(Intent.ExtraProcessText);
            if (!string.IsNullOrEmpty(selectedText)) {
                MpConsole.Instance.WriteLine(@"PROCESS_TEXT: " + selectedText.ToString());

                var intent = new Intent(this, typeof(MainActivity));
                intent.PutExtra("SelectedText", selectedText);
                intent.PutExtra("HostInfo", this.Referrer.Host);

                if(!string.IsNullOrEmpty(this.Referrer.Host)) {
                    var icon = this.ApplicationContext.PackageManager.GetApplicationIcon(this.Referrer.Host);
                    if (icon != null) {
                        var imgView = new ImageView(ApplicationContext);
                        imgView.SetImageDrawable(icon);
                        Bitmap bitmap = BitmapFactory.DecodeResource(this.Resources, Android.Resource.Drawable.PictureFrame);
                        MemoryStream stream = new MemoryStream();
                        bitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);
                        //byte[] bitmapData = stream.ToArray();

                        //var bitmap = ((BitmapDrawable)imgView.Drawable).Bitmap
                        //var baos = new MemoryStream();
                        //bitmap.Compress(Bitmap.CompressFormat.Png, 100, baos);
                        byte[] imageInByte = stream.ToArray();
                        if (imageInByte != null && imageInByte.Length > 0) {
                            intent.PutExtra("HostIconByteArray", imageInByte);
                        }
                    }
                }
                
                StartActivity(intent);
            }
        }
    }
}