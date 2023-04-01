using MonkeyPaste.Common;

namespace MonkeyPaste {
    public interface MpIColorQueryTools {
        int ColorPixelCount(string base64Img, string color, double max_dist = 0);
        bool IsHexColorMatch(string hexFieldStr, string color, double max_dist = 0);
    }
}
