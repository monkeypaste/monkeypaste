using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using MonkeyPaste.Common;
using System;
using System.IO;
using Path = System.IO.Path;

namespace MonkeyPaste.Avalonia.Android {
    public class MpAvAdIconBuilder : MpAvIconBuildBase {
        public override string GetPathIconBase64(string appPath, MpIconSize iconSize = MpIconSize.MediumIcon32) {
            try {
                string package_name = Path.GetFileName(appPath);
                using (Drawable d =
                    MainActivity.Instance.ApplicationContext.PackageManager.GetApplicationIcon(package_name)) {
                    Bitmap bmp = null;
                    if (d is BitmapDrawable bmp_d) {
                        bmp = bmp_d.Bitmap;
                    } else {
                        // from https://stackoverflow.com/a/29091591/105028
                        int width = !d.Bounds.IsEmpty ? d.Bounds.Width() : d.IntrinsicWidth;

                        int height = !d.Bounds.IsEmpty ? d.Bounds.Height() : d.IntrinsicHeight;

                        // Now we check we are > 0
                        bmp = Bitmap.CreateBitmap(
                            width <= 0 ? 1 : width,
                            height <= 0 ? 1 : height,
                            Bitmap.Config.Argb8888);

                        Canvas canvas = new Canvas(bmp);
                        d.SetBounds(0, 0, canvas.Width, canvas.Height);
                        d.Draw(canvas);
                    }
                    if (bmp == null) {
                        return null;
                    }
                    using (var ms = new MemoryStream()) {
                        bmp.Compress(Bitmap.CompressFormat.Png, 100, ms);
                        byte[] bytes = ms.ToArray();
                        string result = bytes.ToBase64String();
                        return result;
                    }

                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error find icon for path '{appPath}'", ex);
                return null;
            }


        }
    }
}
