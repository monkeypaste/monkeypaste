
using Android.Graphics;
using System.IO;
using System.Linq;
using System.Text;
using Java.Lang;
using Java.IO;
using Xamarin.Forms.PlatformConfiguration;
using System;
using Android.Views;
using Xamarin.Forms;
using MonkeyPaste.Plugin;

namespace MonkeyPaste.Droid {
    public class MpScreenshot : MpIScreenshot {
        public static int IMG_COUNT = 0;

        //public byte[] Capture() {
        //    string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);// @"/storage/emulated/0/Download/"
        //    string path = System.IO.Path.Combine(folder, string.Format(@"screen{0}.png",IMG_COUNT++));

        //    Runtime.GetRuntime().Exec(string.Format(
        //        "screencap -p {0}",
        //        path));

        //    var imgBytes = MpHelpers.Instance.ReadBytesFromFile(path);
            
        //    //MpHelpers.Instance.DeleteFile(path);

        //    return imgBytes;
        //}

        //public byte[] Capture2() {
        //    try {
        //        // image naming and path  to include sd card  appending name you choose for file
        //       // String mPath = System.Environment.Get.getExternalStorageDirectory().toString() + "/" + now + ".jpg";
               
        //        // create bitmap screen capture

        //        var v1 = MainActivity.Current.Window.DecorView.RootView; //getWindow().getDecorView().getRootView();
        //        v1.DrawingCacheEnabled = true;
        //        var b = Bitmap.CreateBitmap(v1.DrawingCache);
        //        v1.DrawingCacheEnabled = false;

        //        //Java.IO.File imageFile = new Java.IO.File(path);

        //        byte[] bitmapData; 
        //        using (var stream = new System.IO.MemoryStream()) { 
        //            b.Compress(Bitmap.CompressFormat.Png, 0, stream); 
        //            bitmapData = stream.ToArray(); 
        //        }

        //        return bitmapData;
        //    }            
        //    catch (System.Exception ex) {
        //        MpConsole.WriteTraceLine(ex);
        //        // Several error may come out with file handling or DOM
        //       // e.printStackTrace();
        //    }
        //    return null;
        //}

        public byte[] Capture(object w) {
            if(w == null) {
                return null;
            }
            try { // image naming and path to include sd card appending name you choose for file System.String mPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).ToString() + "/" + now + ".jpg";

                // create bitmap screen capture
                Android.Views.View v1 = ((Window)w).DecorView.RootView;
                if(v1.Width <= 0 || v1.Height <= 0) {
                    return null;
                }
                //v1.DrawingCacheEnabled = true;
                var bitmap = Bitmap.CreateBitmap(v1.Width, v1.Height, Bitmap.Config.Argb8888);
                //v1.DrawingCacheEnabled = false;
                var canvas = new Canvas(bitmap);
                v1.Draw(canvas);

                return GetByteArray(bitmap);
            }
            catch (Throwable e) {
                MpConsole.WriteTraceLine(e);
            }
            return null;
        }

        private byte[] GetByteArray(Bitmap bmp) {
            using (var stream = new MemoryStream()) {
                bmp.Compress(Bitmap.CompressFormat.Png, 0, stream);
                return stream.ToArray();
            }
        }
    }
}