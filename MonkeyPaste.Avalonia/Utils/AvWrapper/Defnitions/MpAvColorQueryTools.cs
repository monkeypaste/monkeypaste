using Avalonia.Media.Imaging;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
//using Avalonia.Win32;

namespace MonkeyPaste.Avalonia {
    public class MpAvColorQueryTools : MpIColorQueryTools {
        public int ColorPixelCount(string base64Img, string colorStr, double max_dist = 0) {
            int count = 0;
            MpColor color = new MpColor(colorStr);
            if (base64Img.ToAvBitmap() is Bitmap bmp) {
                count = bmp.GetPixelColorCount(color, max_dist);
            }
            return count;
        }
    }
}
