using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public static class MpColorHelpers {
        public static string RgbaToHex(byte r, byte g, byte b, byte a = 255) {
            byte[] argb = new byte[] { a, r, g, b };
            return "#" + BitConverter.ToString(argb).Replace("-", "");
        }

    }
}
