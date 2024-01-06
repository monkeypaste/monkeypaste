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
        public bool IsHexColorMatch(string hexFieldStr, string colorStr, double max_dist = 0) {
            if (string.IsNullOrEmpty(hexFieldStr)) {
                return false;
            }
            if (string.IsNullOrEmpty(colorStr)) {
                // return true when no match paramValue provided
                return true;
            }
            var field_color = new MpColor(hexFieldStr).ToPixelColor();
            var match_color = new MpColor(colorStr).ToPixelColor();
            if (match_color.ColorDistance(field_color) <= max_dist) {
                return true;
            }
            return false;
        }
    }
}
