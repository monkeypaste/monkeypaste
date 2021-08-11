
using Android.Graphics;
using System.IO;
using System.Linq;
using System.Text;
using Java.Lang;

namespace MonkeyPaste.Droid {
    public class MpScreenshot : MpIScreenshot {
        public static int IMG_COUNT = 0;

        public byte[] Capture() {
            string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);// @"/storage/emulated/0/Download/"
            string path = System.IO.Path.Combine(folder, string.Format(@"screen{0}.png",IMG_COUNT++));

            Runtime.GetRuntime().Exec(string.Format(
                "screencap -p {0}",
                path));

            var imgBytes = MpHelpers.Instance.ReadBytesFromFile(path);
            
            //MpHelpers.Instance.DeleteFile(path);

            return imgBytes;
        }
    }
}