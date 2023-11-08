using Android.Graphics;
using Android.Graphics.Drawables;
using MonkeyPaste.Common;
using System.IO;
using Path = System.IO.Path;

namespace MonkeyPaste.Avalonia.Android {
    public class MpAvAdIconBuilder : MpAvIconBuildBase {
        public override string GetPathIconBase64(string appPath, MpIconSize iconSize = MpIconSize.MediumIcon32) {
            if (!appPath.IsFileOrDirectory()) {
                return null;
            }
            string package_name = Path.GetFileName(appPath);
            using (Drawable d =
                MainActivity.Instance.ApplicationContext.PackageManager.GetApplicationIcon(package_name)) {
                Bitmap bmp = ((BitmapDrawable)d).Bitmap;
                using (var ms = new MemoryStream()) {
                    bmp.Compress(Bitmap.CompressFormat.Png, 100, ms);
                    byte[] bytes = ms.ToArray();
                    string result = bytes.ToBase64String();
                    return result;
                }

            }


        }
    }
}
